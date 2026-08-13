# 1比1切割控制程序第二阶段

## Goal

复刻图06独立切割控制程序界面；真实设备运动与切割需独立安全验收。

## Requirements

- Inherit the parent 1:1 operator-compatibility and brand-substitution decisions.
- Reproduce the white CAD canvas, millimetre rulers, material rectangle, colored paths, top menus/toolbars and right control panel from `05-图片/06.png`.
- Preserve visible control order: communication/machine address, layer/mode/speed/output table, layer ordering, X/Y jog/home, adsorption, Mark cut, positioning, border trace, pause/resume, stop, load and start.
- Phase 2A is offline deterministic simulation only; commands cannot contact hardware.
- Phase 2B real-device activation requires a separately approved protocol adapter and machine-safety specification.

## Acceptance Criteria

- [ ] Same-size comparison to image 06 passes for layout, text/order, state, colors, rulers and sample paths.
- [ ] Brand is substituted without moving controls.
- [ ] Without an approved adapter, every hardware-affecting command is blocked and visibly offline/simulated, with no socket/serial/process/device side effect.
- [ ] Tests cover disconnected state, load preview, jog/home/start rejection, pause/stop simulation and audit messages.
