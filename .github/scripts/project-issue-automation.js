/**
 * Project v2 Issue Automation (updated priority scheme)
 *
 * Priorities: Critical | High | Medium | Low
 * Sizes: 15 Mins | 30 Mins | 1 Hour | 2 Hours | +4 Hours
 *
 * Logic:
 *  - Adds issue to user project #PROJECT_NUMBER
 *  - If Priority field not yet set, derive from title/body
 *  - Allows explicit override with a line: "Priority: High" (case-insensitive)
 *  - Sets Size if a "Size:" override is found; else default
 */

const owner = process.env.PROJECT_OWNER;
const projectNumber = parseInt(process.env.PROJECT_NUMBER, 10);

const DEFAULT_FEATURE_PRIORITY = process.env.DEFAULT_FEATURE_PRIORITY || "Medium";
const DEFAULT_DOC_PRIORITY = process.env.DEFAULT_DOC_PRIORITY || "Low";
const DEFAULT_BUG_PRIORITY = process.env.DEFAULT_BUG_PRIORITY || "Medium";
const SIZE_DEFAULT = process.env.SIZE_DEFAULT || "15 Mins";

const issue = context.payload.issue;
if (!issue) {
  core.info("No issue payload; exiting.");
  return;
}

// GraphQL: fetch project + fields
const projectQuery = `
query($login:String!, $number:Int!) {
  user(login:$login) {
    projectV2(number:$number) {
      id
      fields(first:50) {
        nodes {
          ... on ProjectV2FieldCommon { id name }
          ... on ProjectV2SingleSelectField { id name options { id name } }
        }
      }
    }
  }
}`;

const projectData = await github.graphql(projectQuery, { login: owner, number: projectNumber });
if (!projectData.user || !projectData.user.projectV2) {
  core.setFailed(`Project #${projectNumber} not found for ${owner}`);
  return;
}

const project = projectData.user.projectV2;
const projectId = project.id;

function getField(name) {
  return project.fields.nodes.find(f => f.name === name);
}
const statusField = getField("Status");
const priorityField = getField("Priority");
const sizeField = getField("Size");

// Utility
function findOption(field, name) {
  if (!field || !field.options) return null;
  return field.options.find(o => o.name.toLowerCase() === name.toLowerCase());
}

async function setSingleSelect(field, name) {
  if (!field) return;
  const opt = findOption(field, name);
  if (!opt) {
    core.warning(`Option '${name}' not found in field '${field.name}'`);
    return;
  }
  const mutation = `
  mutation($projectId:ID!, $itemId:ID!, $fieldId:ID!, $optionId:String!) {
    updateProjectV2ItemFieldValue(input:{
      projectId:$projectId,
      itemId:$itemId,
      fieldId:$fieldId,
      value:{ singleSelectOptionId:$optionId }
    }) { clientMutationId }
  }`;
  await github.graphql(mutation, {
    projectId,
    itemId,
    fieldId: field.id,
    optionId: opt.id
  });
  core.info(`Set ${field.name} -> ${opt.name}`);
}

// Get or add project item
const itemLookupQuery = `
query($projectId:ID!) {
  node(id:$projectId) {
    ... on ProjectV2 {
      items(first:200) {
        nodes {
          id
          content { ... on Issue { id number } }
        }
      }
    }
  }
}`;
const itemsData = await github.graphql(itemLookupQuery, { projectId });
let itemId = null;
for (const it of itemsData.node.items.nodes) {
  if (it.content && it.content.id === issue.node_id) {
    itemId = it.id;
    break;
  }
}
if (!itemId) {
  const addMutation = `
  mutation($projectId:ID!, $contentId:ID!) {
    addProjectV2ItemById(input:{projectId:$projectId, contentId:$contentId}) {
      item { id }
    }
  }`;
  const added = await github.graphql(addMutation, { projectId, contentId: issue.node_id });
  itemId = added.addProjectV2ItemById.item.id;
  core.info(`Added issue #${issue.number} to project.`);
} else {
  core.info(`Issue #${issue.number} already in project.`);
}

// Parse body overrides
const body = issue.body || "";
function matchLine(prefix) {
  const r = new RegExp(`^\\s*${prefix}:\\s*(.+)$`, 'im');
  const m = body.match(r);
  return m ? m[1].trim() : null;
}
const overridePriority = matchLine("Priority");
const overrideSize = matchLine("Size");

// Derive type
const title = issue.title || "";
const labels = (issue.labels || []).map(l => l.name.toLowerCase());
const isBug = title.startsWith("[BUG]") || labels.includes("bug");
const isFeature = title.startsWith("[FEATURE]") || labels.includes("feature");
const isDoc = title.startsWith("[DOC]") || labels.includes("documentation") || labels.includes("doc");

// Heuristic priority if not overridden
let derivedPriority = overridePriority;
if (!derivedPriority) {
  if (isBug) {
    const lowerBody = body.toLowerCase();
    if (/(crash|exception|freeze|hang|infinite loop|fails to generate|cannot generate)/.test(lowerBody)) {
      derivedPriority = "Critical";
    } else if (/(misalign|wrong coordinate|axis mix|not optimal path|incorrect path|suboptimal)/.test(lowerBody) ||
               /(priorityqueue|pathfindinggraphgenerator)/i.test(title)) {
      derivedPriority = "High";
    } else if (/(duplicate|mismatch|seed|inconsistent)/.test(lowerBody)) {
      derivedPriority = "Medium";
    } else {
      derivedPriority = DEFAULT_BUG_PRIORITY;
    }
  } else if (isFeature) {
    derivedPriority = DEFAULT_FEATURE_PRIORITY;
  } else if (isDoc) {
    derivedPriority = DEFAULT_DOC_PRIORITY;
  } else {
    derivedPriority = "Medium"; // fallback
  }
}

// Derive size
let derivedSize = overrideSize || SIZE_DEFAULT;

// Set Status to Todo if available (fire-and-forget)
if (statusField) {
  const todoOpt = findOption(statusField, "Todo");
  if (todoOpt) {
    await setSingleSelect(statusField, "Todo");
  }
}

// Apply Priority & Size (only if those fields exist)
if (priorityField) await setSingleSelect(priorityField, derivedPriority);
if (sizeField) await setSingleSelect(sizeField, derivedSize);

core.info("Automation complete.");