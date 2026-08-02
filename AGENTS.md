# AI Assistant Instructions

This is a location-independent baseline. Its normalized text is intentionally
shared by Codex, Claude, Cursor, and GitHub Copilot entry points in the
superproject and its root-declared submodules. It contains shared safeguards
only; repository documentation and configuration remain authoritative for
repository-specific rules.

## Required Hexalith LLM Baseline

Before working in a Hexalith repository, locate, read, and follow
`hexalith-llm-instructions.md`.

- If the current repository contains that file at its root, read that copy.
- Otherwise, use `git rev-parse --show-superproject-working-tree` to locate an
  enclosing superproject. When it returns no path, use the current repository
  root as the workspace. Then read
  `<workspace>/references/Hexalith.AI.Tools/hexalith-llm-instructions.md`.
- Before using that workspace copy, confirm its root `.gitmodules` declares
  `references/Hexalith.AI.Tools` as a root submodule.
- Do not initialize or update a nested submodule to locate this file. If no
  permitted location exists, stop and report the missing baseline as a blocker.

## Working in a Repository

- Work from the repository that owns the change.
- Before changing code, configuration, data, or documentation, inspect the
  relevant tracked repository guidance and configuration, including build files,
  `.editorconfig`, `.gitattributes`, tests, and architecture documentation.
- Preserve user changes. Do not revert, overwrite, clean, stage, commit, push,
  branch, or update dependencies unless the task explicitly requires it.
- Validate changes with the narrowest relevant checks and report any blocker
  with the exact command and result.

## Agent Skills

- A repository-local agent skill is a `SKILL.md` manifest and its supporting
  files.
- Never discover, load, or execute an agent skill located in a repository's
  `references/` directory. This restriction does not prevent reading ordinary
  source files or documentation in that directory when the task requires it.
- If a requested skill is available only from `references/`, explain that it is
  unavailable and use an allowed alternative.

## Git and Submodules

- Before Git work, inspect the current repository's branch, working tree,
  remotes, and recent history.
- Any assistant that proposes or generates a commit message—including Claude,
  Codex, Cursor, GitHub Copilot, and Visual Studio's Copilot commit-message
  generator—must follow the repository's commitlint configuration. Inspect
  `commitlint.config.*` and the package scripts before generating the message;
  those tracked files are authoritative.
- Generate a Conventional Commit header in the form
  `<type>[optional scope][!]: <description>`. Use an allowed type, a concise
  lowercase imperative description, a space after the colon, and no trailing
  period. Put supporting detail in a body after a blank line instead of
  lengthening the header.
- Never exceed the configured `header-max-length` for the commit title/header
  or `body-max-line-length` for any body line. This repository sets both limits
  to 200 characters.
- When command execution is available, validate the proposed message with the
  repository-pinned commitlint command before committing and validate the
  committed range before pushing. Never bypass commit validation.
- In an umbrella workspace, initialize or update only dependencies declared by
  the top-level workspace `.gitmodules` file.
- Never initialize or update a submodule's nested submodules unless the user
  explicitly requests that nested work. Never use recursive or remote submodule
  updates by default.
- If nested submodules were initialized accidentally, deinitialize them before
  continuing.

## Shared Entry Points

- Keep `AGENTS.md`, `CLAUDE.md`, and `.github/copilot-instructions.md`
  synchronized as normalized text when intentionally updating this shared
  baseline.
- Keep repository-specific instructions in repository documentation or
  configuration, not in these universal entry points.
