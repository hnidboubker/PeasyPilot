# PROJECT_INDEX.md

GPS for what to read depending on the task at hand. Start with `AI_CONTEXT.md` for
a fast orientation, then come here to find the right file for the job.

## "I want to understand the project quickly"

→ `AI_CONTEXT.md` (2–5 min read), then `.agents/05_ARCHITECTURE.md` for detail.

## "I need to follow the rules before doing anything"

→ `PROJECT_MEMORY.md` → `.agents/00_START_HERE.md` → `.agents/02_QUESTION_PROTOCOL.md`
→ `.agents/01_RULES.md` (full mandatory order in `.agents/00_START_HERE.md`).

## "I'm about to write or change C#/.NET code"

→ `.agents/04_LANGUAGE_SPECIFIC.md` (conventions, package boundaries)
→ `.agents/05_ARCHITECTURE.md` (where the change belongs)

## "I'm about to commit"

→ `.agents/03_CHECKLIST_BEFORE_COMMIT.md`
→ `.agents/07_AUDIT_REQUIREMENTS.md`
→ `.agents/01_RULES.md` (git stays human-controlled)

## "A bug, error, or failing test just showed up"

→ `.agents/08_AUTO_ISSUE_SKILL.md` (mandatory sequence: issue first, then fix)
→ `.agents/09_MCP_GITHUB_CONFIG.md` (how auto-issue creation is wired — not yet
   configured for this repo)
→ `.agents/10_COMPLETE_WORKFLOW.md` (the full three-skill chain, end to end)

## "I want to know what skills/automations exist"

→ `.agents/06_SKILLS_AVAILABLE.md`

## "I'm updating AI_CONTEXT.md or this file"

→ `.agents/11_MAINTENANCE_AI_CONTEXT_INDEX.md`

## "I want the project history / past decisions"

→ `PROJECT_MEMORY.md` (rotates to `PROJECT_MEMORY_01.md`, etc. at 300 lines — check
   `Next` at the bottom of the current file if it looks incomplete)

## File map

| File | Purpose |
|---|---|
| `CLAUDE.md` | Project governance entry point (reading order, core rules) |
| `AGENTS.md` | Pointer to `CLAUDE.md`, for tools that look for `AGENTS.md` |
| `AI_CONTEXT.md` | Fast project orientation |
| `PROJECT_INDEX.md` | This file — task → file routing |
| `PROJECT_MEMORY.md` | Decisions, open questions, history |
| `.agents/00_START_HERE.md` | Reading order, ownership |
| `.agents/01_RULES.md` | Non-negotiable rules |
| `.agents/02_QUESTION_PROTOCOL.md` | When/how to ask before acting |
| `.agents/03_CHECKLIST_BEFORE_COMMIT.md` | Pre-commit checklist |
| `.agents/04_LANGUAGE_SPECIFIC.md` | C#/.NET conventions |
| `.agents/05_ARCHITECTURE.md` | Package layout and responsibilities |
| `.agents/06_SKILLS_AVAILABLE.md` | Skills registered for this project |
| `.agents/07_AUDIT_REQUIREMENTS.md` | What "done" must satisfy |
| `.agents/08_AUTO_ISSUE_SKILL.md` | Bug → issue → fix → commit sequence |
| `.agents/09_MCP_GITHUB_CONFIG.md` | GitHub MCP token setup for auto-issue |
| `.agents/10_COMPLETE_WORKFLOW.md` | The three skills chained end to end |
| `.agents/11_MAINTENANCE_AI_CONTEXT_INDEX.md` | Keeping this file and AI_CONTEXT.md current |
