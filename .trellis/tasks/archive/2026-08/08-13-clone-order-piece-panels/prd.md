# 1比1订单与裁片参数区

## Goal

Reproduce the order/group tree, high-density piece cards, footer progress, and batch property table.

## Requirements

- Inherit parent decisions and use images 08–10, 13, and 27.
- Preserve every evidenced card field and exact field order: thumbnail, size, bounding dimensions, rotation, completion, 单套/套数/余量/总量.
- Implement the image-13 property dialog/table as the dense batch editor; unknown formulas remain evidence gaps.

## Acceptance Criteria

- [ ] Six visible cards and summaries match image 27 geometry and deterministic values.
- [ ] Property dialog matches image 13 column order, split ratio, default checks, values, and focus state.
- [ ] Edits update a shared order/piece state used by the main shell.

## Notes

- Keep `prd.md` focused on requirements, constraints, and acceptance criteria.
- Lightweight tasks can remain PRD-only.
- For complex tasks, add `design.md` for technical design and `implement.md` for execution planning before `task.py start`.
