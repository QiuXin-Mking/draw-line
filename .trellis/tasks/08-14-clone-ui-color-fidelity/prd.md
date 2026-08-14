# 1比1 UI 色彩统一

## Goal

将当前 Avalonia 固定工作台的颜色从偏绿、偏暖和过度现代化的演示主题，统一为竞品截图证实的经典 Windows 高密度色彩系统，使标题栏、菜单、工具栏、订单/裁片区、画布、标尺、右侧面板、状态栏及后续模态窗口在视觉温度和状态色上保持一致。

## Background and Evidence

- 父 PRD 已明确 1:1 操作员兼容优先，不允许原创视觉重设计。
- 主界面证据以 `05-图片/10.png`、`14.png`、`16.png`、`21.png`、`27.png` 为主，其他截图用于交叉检查状态色。
- 当前 `AppTheme` 的 `ClassicTitleBackground #174D46`、`ClassicPanelBackground #EFF0EC`、`ClassicHeaderBackground #D7DED9` 和 `DemoPanelBackground #D5EFEE` 带明显绿色或暖灰偏色。
- 对 `27.png` 原始像素区域的只读取样显示：业务标题栏约 `#1B3030`，菜单接近 `#FEFEFF`，工具栏约 `#EEF0F2`，普通面板接近 `#FFFFFF`，中性表头约 `#D9D9D9`，裁片卡主色约 `#98D4EF`，进度条约 `#51B2C4`，工具图标主色约 `#469589`。
- 新安装的 `screenshot` 与 `frontend-app-builder` 用于原生窗口截图和色彩忠实度验收；技能的原创/现代化默认建议不适用于本任务。

## Requirements

### R1 Evidence-locked palette

- 建立集中、可测试的截图证据色板，不允许各复刻组件继续散落近似颜色。
- 第一轮锁定以下基准令牌；视觉验收可在原图压缩/抗锯齿误差内微调，但不得改变色相方向：

| Token role | Target |
| --- | --- |
| application title | `#1B3030` |
| menu surface | `#FEFEFF` |
| toolbar surface | `#EEF0F2` |
| primary panel surface | `#FFFFFF` |
| neutral header / status surface | `#D9D9D9` / `#F0F0F0` |
| thin classic border | neutral gray, derived from screenshot edges |
| toolbar icon teal | `#469589` |
| piece-card cyan | `#98D4EF` |
| progress cyan | `#51B2C4` |
| CAD canvas | `#000000` |
| ruler chrome | deep neutral charcoal, not green-tinted |
| primary text | near-black neutral |
| disabled text/control | screenshot-neutral gray |

- 白色区域必须保持真白/近真白，不替换成奶白、暖灰或绿色灰。

### R2 Surface coverage

- 统一固定主窗口的标题栏、一级菜单、图标工具栏、左右面板标题、普通面板、裁片卡、进度条、CAD 黑画布、标尺和底部状态栏。
- 已完成的 CAD 工作台、订单/裁片面板与属性窗口必须改为使用统一令牌。
- 后续“版型设置”“排版设置”等经典模态窗口直接复用本色板，不再自行决定色彩。
- 非当前固定主窗口使用的旧模块页面不做全面重设计；只有共享令牌自然影响到的区域随之更新。

### R3 State colors

- 默认、悬停、焦点、选中、禁用、警告和危险状态分别建立明确令牌或组件变体。
- 选中裁片卡仍保持截图中的浅蓝/青色层级；不可使用当前现代蓝色 `Accent #4F9DF2` 代替经典焦点边框。
- TODO/证据缺口提示必须可辨，但不能以大面积橙色破坏截图主体；仅用于文字或小型状态提示。
- 黑色画布上的红边界、白外轮廓、绿内线和彩色裁片属于画布语义色，不与应用 chrome 色板混用。

### R4 No visual redesign

- 不改变五区比例、控件顺序、窗口密度、字号、圆角、阴影和交互入口。
- 不增加渐变、毛玻璃、阴影卡片、现代品牌色覆盖或暗色侧栏。
- 不使用 Image Gen 创造新界面概念；原始竞品截图就是已接受的视觉规格。

### R5 Verification

- 使用固定 1366×768 应用窗口场景采集实现截图，并与对应竞品工作区裁切图并排检查。
- 色彩账本至少检查：标题/菜单/工具栏、左侧裁片卡、CAD/标尺、右侧面板、状态栏/进度条五组区域。
- 自动化测试锁定核心令牌值和复刻组件对共享令牌的使用，防止后续重新引入偏色硬编码。

## Acceptance Criteria

- [ ] `AppTheme` 或等价设计系统提供证据色板和状态变体，核心复刻组件不再自行硬编码近似 chrome 色。
- [ ] 标题栏不再呈当前高饱和绿色；菜单/面板恢复截图中的真白和中性灰。
- [ ] 裁片卡从偏浅绿灰统一为截图青蓝，进度条、工具图标、焦点和选中状态互不混用。
- [ ] CAD 黑画布、深色标尺及白/绿/红几何语义色保持清晰，应用面板色不污染画布。
- [ ] 1366×768 截图对比账本覆盖至少五个区域，并记录所有仍存在的有意偏差。
- [ ] 不改变布局、文案、入口或业务状态；现有功能测试无回归。
- [ ] Desktop 测试、全解决方案测试、构建和 `git diff --check` 通过。

## Out of Scope

- 原创视觉方向、现代化改版、品牌营销配色和新图形资产。
- 布局比例、字体体系、图标造型和业务交互重构；它们由其他复刻/集成任务处理。
- 自动排样结果、材料设置功能和发送流程实现。
- 对采集画面中的 macOS、ToDesk、Windows 任务栏或输入法浮层取色。

## Open Questions

无。父 PRD 已决定采用截图证据优先的 1:1 色彩方案。
