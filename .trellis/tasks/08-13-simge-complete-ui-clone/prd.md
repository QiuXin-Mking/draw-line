# 银象排样软件完整界面复刻

## Goal

基于 `05-图片/` 的 27 张竞品截图与银象科技语音资料，将当前 Avalonia 排样软件改造成操作入口、窗口分区、字段顺序、信息密度和主要操作路径与现有工厂界面一致的迁移兼容版本，使原系统操作员无需重新寻找入口即可上手。

## Highest-Priority Product Decision: 1:1 Operator Compatibility

> **This decision overrides earlier project guidance that required an original visual system or rejected pixel-level comparison.**

- 本项目必须以 **1:1 界面复刻** 为产品目标，不采用原创视觉重设计。
- 决策原因：大部分目标用户已经长期使用截图中的界面，形成了固定的视觉识别和操作肌肉记忆；改变入口位置、窗口分区、字段顺序、标签、颜色编码或操作路径，会导致操作员找不到功能并增加生产误操作风险。
- 1:1 的含义包括：窗口位置与比例、停靠关系、菜单与工具栏顺序、图标语义、按钮位置、字段标签与顺序、列表密度、默认选中状态、画布颜色与标尺、状态栏信息、弹窗层级、关键操作步骤及快捷入口。
- 所有子任务必须继承本决策。子任务不得以“现代化”“更简洁”“原创设计”“改善审美”或框架默认样式为理由，擅自移动、合并、隐藏、重命名或重新设计已由截图/语音证实的控件。
- 视觉验收必须使用对应竞品截图做同尺寸对照；仅通过功能测试但明显改变界面布局，不视为完成。
- 对截图看不清、不同截图互相矛盾或语音未证实的区域，必须登记为“证据待补”，不得自行创造看似合理的新入口。可使用明确 TODO 占位，但占位不得改变已证实区域的布局。
- 功能实现仍须诚实：1:1 外观不等于允许伪造算法、设备通信、文件输出或成功状态；尚未接入的业务能力必须保持 TODO/禁用/限制说明。
- 品牌边界：保留所有影响操作员肌肉记忆的控件几何、位置、颜色和交互，但将竞品名称、Logo、序列号及专有身份标识替换为本产品品牌；不得直接复制竞品品牌资产。

## Evidence Priority

发生冲突时按以下顺序裁决：

1. 用户对本父 PRD 的最新明确决策；
2. `05-图片/` 中对应界面的原始截图；
3. `02-竞品数据分析/02-银象科技/语音正式转文字/` 的正式转写；
4. 原版语音文字和其他竞品研究资料；
5. 当前实现与旧任务文档。

旧父任务 `08-13-image-evidence-requirements` 中“必须采用原创视觉系统”“不做像素级 UI 比对”等约束，与本决策冲突的部分不适用于本任务及其子任务。

## Requirements

- 完整转写 27 张截图，逐图记录布局、可见文字、字段、数值、控件状态、颜色、相对尺寸、功能推断和证据缺口。
- 建立主界面、CAD/导入、码齿、排样运行、排样结果、切割交接及设置弹窗的界面地图与工作流地图。
- 把可独立截图验收的区域拆成 Trellis 子任务，每个子任务引用具体图片与转写行号。
- 建立统一的 1:1 截图验收方法，至少覆盖 1366×768 目标窗口、区域几何、控件顺序、可见文案和关键状态。
- 保留截图证实的“固定主窗口 + 模态参数弹窗”操作模型，不把排样设置、发送设置等擅自改造成独立现代化页面。

## Complete Product Surface

### P1 Fixed main window

- One application window simultaneously hosts the title/menu/large-icon command rows, order tree, piece cards, black CAD/nesting canvas, result list, output statistics, and status bar.
- The five body regions shown in `27.png` are persistent panes, not separate navigation pages: upper-left order/group tree, lower-left piece panel, center canvas, upper-right material/layout results, lower-right output information.
- The main window uses compact classic Windows desktop density, one-pixel borders, small Chinese labels, cyan/green commands, black canvas, and ruler ticks. Framework-default spacious cards are forbidden where the reference shows dense rows.

### P2 Orders and pieces

- The order pane includes the current order/tree, group actions, selected size/group, and piece count.
- Each piece row includes enable, visibility, thumbnail, size, bounding dimensions, rotation rule, completion state, `单套`, `套数`, `余量`, and `总量` in the reference order.
- Footer summaries include total count/area, group progress, total-order progress, and their cyan progress bars.
- The batch `属性` dialog from `13.png` presents the same data in a dense editable table and preserves its column order.

### P3 CAD import and editing

- `CAD工具` opens the existing DXF-first file selection/import route. After import, the fixed shell shows the CAD canvas, left piece pane, and right CAD property pane concurrently.
- Support the evidenced visual states: empty canvas, white-line imported geometry, classified colored lines, blue-gray selection fill, dashed selection, and imported piece cards.
- Reproduce the evidenced drawing toolbar entries/tooltips (`范围缩放`, `绘制多段线`, `绘制矩形`, `导到订单 Ctrl+T`, selection mode `ESC`) and the right-side field order/default states from images 04–12 and 21.

### P4 Materials and nesting settings

- `版型设置` is a modal dialog over the fixed main window with name, direction, material width/length/layers, multi-layer remainder, edge allowance, and piece gap.
- `排版设置` is a modal dialog with time limit, piece group, angle, micro-angle, piece type/order, concavity ratio, and four evidenced options.
- Do not infer undocumented dropdown values, formulas, ranges, or algorithm effects; preserve visible values and mark unsupported behavior honestly.

### P5 Nesting results and output information

- Center canvas renders material boundary and dense colored piece placement at the same default zoom relationship as the evidence; it must not automatically stretch narrow roll material to fill the canvas.
- Upper-right list shows layout/material count, colored status header rows, compact usage thumbnails, and selected row state.
- Lower-right `排版输出信息` shows the yellow/cyan pie chart with central percentage, legend, material dimensions, area, piece count, consumption, and elapsed time.
- The list-row percentage and pie-chart percentage remain separate values because current evidence does not establish a common formula.

### P6 Sending and output handoff

- `发送设置` remains a modal dialog and preserves the fields/order from `22.png`.
- Supported output formats and real file writes must match implemented adapters. Unimplemented formats remain disabled/TODO even when their visual entries are reproduced.
- Generated output appears in a selected output folder with explicit success/failure; no fake device or file success.

### P7 State fidelity

- Provide deterministic visual scenarios for import, CAD classification, selection, order population, nesting settings, running/cancelled/completed nesting, multiple layout candidates, and send settings.
- Each scenario must reproduce the corresponding screenshot values without claiming they came from a real algorithm when they are fixtures.
- Enable/disable states, selection, focus, checkbox/radio defaults, modal ownership, and status text are part of the 1:1 contract.

## Child Task Map

| Child | Responsibility | Primary evidence |
| --- | --- | --- |
| `08-13-clone-shell-frame` | Fixed five-pane shell, title/menu/toolbars, rulers/status, brand substitution | 01–05, 10, 21, 27 |
| `08-13-clone-cad-workbench` | DXF picker/import states, canvas tools, right CAD pane | 01–12, 21 |
| `08-13-clone-order-piece-panels` | Order tree, piece cards, summaries, property table | 08–10, 13, 27 |
| `08-13-clone-material-nesting-settings` | Material/layout dialog and nesting settings modal | 14, 16, 19, 20, 26 |
| `08-13-clone-nesting-result-dashboard` | Dense placement canvas, result list, pie chart, progress/state | 14–18, 25, 27 |
| `08-13-clone-send-handoff` | Send settings and file handoff | 22–24 |
| `08-13-clone-visual-integration` | Scenario integration and screenshot-diff acceptance | all product-scope images |
| `08-13-clone-cutting-control` | Phase 2 separate cutting-control UI; hardware gated | 06 |

## Acceptance Criteria

- [ ] 父 PRD 明确记录 1:1 复刻为最高优先级产品决策，并声明覆盖冲突的旧视觉约束。
- [ ] 27 张截图均有独立、可追溯的界面转写。
- [ ] 每个已证实的入口、窗口、字段和操作路径均归属一个子任务或明确的父级集成验收项。
- [ ] 子任务验收均包含对应截图的同尺寸视觉对照，不以主观“风格接近”代替。
- [ ] 未证实能力保持证据待补或 TODO，不伪造生产功能。
- [ ] 竞品品牌资产均已替换，但替换不改变控件尺寸、位置或操作路径。
- [ ] `27.png` five-pane layout is visible simultaneously in one product window, including all left-card and right-statistics fields described in `research/images-19-27.md`.
- [ ] CAD, property, material, nesting, and send dialogs/states can be reached through the evidenced entry paths.
- [ ] Every child task has a screenshot-evidence contract and the final integration task verifies the combined window rather than isolated pages.

## Phase Boundary

- Phase 1 delivers the main nesting application and its file handoff.
- Phase 2 delivers the separate cutting-control application shown in `06.png`.
- Phase 2 first reproduces UI and offline simulation. Real communication, jog, homing, positioning and cutting remain disabled until protocol, emergency-stop, interlock, permissions, timeout, audit and machine-side acceptance are separately approved.
