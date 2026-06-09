# Git / Vercel / Publish Plan

This checklist tracks the full publishing goal for the BaseProject Solitaire WebGL release.

## 0. Current Scope

- [x] Preserve the current dirty BaseProject worktree until all user-visible Solitaire work is validated.
- [x] Keep BaseProject rules active: no new managers, no duplicate event bus, no scene rebuild, no `FormerlySerializedAs`.
- [x] Treat local Unity/test/build output and deployed Vercel behavior as authoritative evidence.

## 1. Feature Completion

- [x] Score is implemented through global `EventManager.SolitaireEvents` instead of direct controller references.
- [x] `ScoreTextManager` listens to score events and updates TMP text from a `SolitaireScoreConfigSO`.
- [x] Undo UI is present in the current worktree and should stay visible/enabled.
- [x] Hint/AutoComplete implementation subagent created. Evidence: pending worktree `local:7cc69f20-8539-45c1-8c77-894c37bb353f`.
- [x] Hint system exists, cycles legal moves, and only suggests/highlights moves. Evidence: `SolitaireHintService`, `SolitaireHintPresenter`, `SolitaireHintButton`, and EditMode hint tests.
- [x] AutoComplete system exists, detects safe deterministic foundation completion, and executes through existing move paths. Evidence: `SolitaireDeckController.TryAutoCompleteToFoundation` uses existing `TryAutoMoveToFoundation` path.
- [x] Hint and AutoComplete are tested and validated after subagent work is merged. Evidence: EditMode 16/16 passed and PlayMode 4/4 passed on 2026-06-09 via TestRunner API XML.

## 2. UI Orientation Validation

- [x] Portrait/landscape UI validation subagent created. Evidence: pending worktree `local:f7c648f5-2819-41d2-be85-0ae5a71d5727`.
- [ ] Portrait gameplay screenshot captured after latest UI revisions.
- [ ] Landscape gameplay screenshot captured after latest UI revisions.
- [x] Score, undo, moves, stock/waste/foundation/tableau, and game state UI do not overlap in portrait. Evidence: HUD controls changed to two-row portrait layout; 390x844 bounding boxes returned no overlaps.
- [x] Score, undo, moves, stock/waste/foundation/tableau, and game state UI do not overlap in landscape. Evidence: landscape stack spacing changed to 0.145 and 844x390 bounding boxes returned no overlaps.
- [ ] Real gameplay screenshots still need GUI/MCP or WebGL capture; batch Unity can validate rects but did not produce GameView PNG.
- [ ] UI remains playable when hosted in a resizable WebGL wrapper.

## 3. Automated Tests

- [x] Unity compile check passes with no C# errors. Evidence: Unity console error check returned 0 errors on 2026-06-09.
- [x] Existing Solitaire EditMode tests pass. Evidence: EditMode 16/16 passed on 2026-06-09 in `/tmp/baseproject-test-results/api-editmode.xml`.
- [x] Existing Solitaire PlayMode tests pass. Evidence: PlayMode 4/4 passed on 2026-06-09 in `/tmp/baseproject-test-results/api-playmode.xml`.
- [x] New score tests pass. Evidence: included in EditMode 16/16 passed on 2026-06-09.
- [x] New hint tests pass. Evidence: included in EditMode 16/16 passed on 2026-06-09.
- [x] New autocomplete tests pass. Evidence: included in EditMode 16/16 passed on 2026-06-09.
- [x] Any UI/layout tests or screenshot validations pass. Evidence: prefab wiring completed after HUD layout revisions; EditMode 16/16 and PlayMode 4/4 still pass, plus portrait/landscape HUD bounding-box checks returned no overlaps.

## 4. Benchmarks

- [x] `Tools/Solitaire/Run Benchmarks` runs for the current score/undo baseline. Evidence: benchmark run completed on 2026-06-09 in Unity `6000.3.16f1`.
- [x] README benchmark numbers are updated only with measured results. Evidence: README updated with 2026-06-09 benchmark run values.
- [x] Score event dispatch/update benchmark is reported. Evidence: 500,000 iterations, 3.463 ms total, 0.007 us average, 144,366,807 ops/s on Apple M3 / OSXEditor.
- [x] Hint enumeration benchmark is reported. Evidence: 100,000 iterations, 268.426 ms total, 2.684 us average, 372,542 ops/s.
- [x] AutoComplete eligibility/execution benchmark is reported. Evidence: 20,000 iterations, 818.944 ms total, 40.947 us average, 24,422 ops/s.
- [ ] FPS/render metrics are included only if captured from a stable runtime session.

## 5. README / Docs

- [x] GitHub `README.md` describes currently implemented local features honestly.
- [ ] README includes architecture, test matrix, benchmark evidence, WebGL/Vercel link, and validation notes.
- [x] README does not claim FPS/Vercel before verified.
- [ ] Relevant docs are updated if feature contracts change.

## 6. WebGL Builds

- [ ] WebGL build settings are inspected and corrected if needed.
- [ ] Portrait WebGL build produced or a single responsive build is proven to support portrait.
- [ ] Landscape WebGL build produced or a single responsive build is proven to support landscape.
- [ ] Build artifacts are smoke-tested locally.
- [ ] Build artifacts are arranged for Vercel hosting.

## 7. Vercel Wrapper / Deployment

- [ ] Vercel project structure is created outside Unity build internals.
- [ ] WebGL canvas wrapper supports selectable/resizable game screen sizes.
- [ ] Refresh/reload controls are present if useful for testers.
- [ ] Vercel deployment succeeds on the user's connected account.
- [ ] Deployed URL is opened with browser/computer-use verification.
- [ ] Portrait-size deployed game is playable with no critical UI overlap.
- [ ] Landscape-size deployed game is playable with no critical UI overlap.

## 8. GitHub Publication

- [ ] Remove old BaseProject git remote/linkage only after local validation and before new publish.
- [ ] Create a new public GitHub repository under the user's GitHub account.
- [ ] Add the new remote and push the final validated project.
- [ ] Verify GitHub README renders correctly with Vercel link and validation evidence.

## 9. Final Completion Audit

- [ ] All explicit user requirements above have direct evidence.
- [ ] Subagent outputs have been reviewed and integrated or explicitly rejected with reason.
- [ ] No required validation is missing or only assumed.
- [ ] Final response reports files changed, tests, builds, Vercel URL, GitHub URL, and any caveats.
