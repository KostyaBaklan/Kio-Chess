# Chess Engine Repository Instructions

## Project
- Classical chess engine written in C# using bitboards.
- Primary goals are playing strength, search correctness, and performance.
- Typical search depth is 8-12 plies; prioritize techniques effective at these depths.
- Do not suggest NNUE, large training datasets, cluster testing, or deep-search-only techniques unless explicitly requested.

## Existing Search Features
- Alpha-beta search
- Transposition tables
- Null-move pruning
- Late move reductions
- Relative history heuristics
- Killer moves
- Quiescence search

Do not propose an existing feature as a new implementation. Improvements or tuning of these features are allowed.

## Recommendations
When proposing a search or evaluation change:
- Explain its expected playing-strength value and speed impact.
- State whether it is useful around depth 10.
- Identify interactions with existing pruning, reductions, move ordering, and transposition-table logic.
- Describe correctness risks and possible tactical or positional regressions.
- Treat Elo estimates as approximate and distinguish evidence from speculation.
- Prefer practical improvements suitable for a standalone classical engine.

## Implementation
- Match the existing local coding and naming style.
- Prefer the smallest safe change; do not refactor unrelated code.
- Do not rename symbols or reformat unaffected code without necessity.
- Avoid unnecessary abstractions and complex design patterns.
- Provide complete, directly usable C# code when code is requested.
- Identify the exact file, class, and method to change when known from the available context.

## Hot-Path Performance
For search, move generation, make/unmake, and evaluation:
- Avoid heap allocations, LINQ, boxing, reflection, and avoidable virtual dispatch.
- Avoid unnecessary board-state copies and temporary collections.
- Prefer reusable buffers, value types, and stack allocation when safe and measurably beneficial.
- Consider cache locality, branch cost, and computational overhead.
- Do not sacrifice search correctness for an unmeasured micro-optimization.

## Search Correctness
- Preserve alpha-beta bounds and principal-variation behavior.
- Preserve mate-score normalization and distance-to-mate semantics.
- Preserve transposition-table bound, depth, age, and replacement semantics.
- Preserve repetition, draw, legality, and terminal-position detection.
- Be conservative near the horizon, in check, in tactical positions, and in zugzwang-prone endgames.

## Debugging
1. Diagnose the root cause from supplied code, logs, positions, and test results.
2. Separate confirmed findings from hypotheses.
3. Explain the failure mechanism before proposing a fix.
4. Recommend the smallest safe correction.
5. State performance, strength, and regression risks.

## Testing
- Provide relevant FEN positions and expected behavior when useful.
- Suggest focused regression tests for changed search or evaluation behavior.
- Do not claim an Elo gain without appropriate match testing.
- Do not create tests or benchmark infrastructure unless requested.

## File and Action Boundaries
- Do not create Markdown, README, design, architecture, report, plan, benchmark, changelog, release-note, or other documentation/text files unless explicitly requested.
- Present explanations, plans, reviews, FEN positions, and analysis in chat unless a file is explicitly requested.
- Do not create sample projects, demos, playgrounds, helper classes, or new source files unless requested or clearly necessary.
- Prefer safely modifying existing files over creating new ones.
- Do not edit generated files unless explicitly requested.
- Do not commit, push, publish, or modify repository history.
- Stop after completing the requested task.

## Response Style
- Be direct, concise, technical, and evidence-based.
- Avoid generic chess-programming explanations unless requested.
- For patches, summarize the core logic and affected behavior briefly.
