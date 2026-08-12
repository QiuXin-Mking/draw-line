# 阶段 1 验收记录

实施范围：项目骨架、DXF 检查器、项目容器 v1、导入向导状态与样本清单。

| 用例 | 自动化资产 | 当前状态 |
|---|---|---|
| P1-BLD-001 | solution、严格构建属性、层级项目引用 | 通过：Desktop Release 构建，0 warning / 0 error；依赖以本地校验缓存恢复 |
| P1-DXF-001 | `AsciiDxfReaderTests.Sandal_fixture_has_nine_closed_polyline_candidates`、`Closed_legacy_polyline_is_a_piece_candidate` | 待本机 .NET SDK 可用后复验：闭合且完整的 LWPOLYLINE/旧式 POLYLINE 都可作为候选，保留其实体类型、顶点数与图层。|
| P1-DXF-002 | `Legacy_fixture_reports_open_polyline_without_silent_closure` | 通过：2026-08-11 本机 .NET 10，38–45.DXF 全部样本 |
| P1-DXF-003 | `ProjectDocumentTests.Commit_import_records_unit_decision...` | 通过：2026-08-11 本机 .NET 10 |
| P1-DXF-004 | `Invalid_file_returns_a_diagnostic_instead_of_throwing`、`Unterminated_legacy_polyline_is_reported_without_becoming_a_candidate`、`Missing_source_after_inspection_returns_a_blocking_diagnostic_without_throwing` | 待本机 .NET SDK 可用后复验：无效文件、缺少 `SEQEND` 的旧式 POLYLINE，或检查后源文件不可再读取时，只产生可定位阻断诊断，不抛出异常或伪造裁片；缺少 SHA-256 来源指纹时不得提交导入。|
| P1-DXF-005 | `Legacy_fixture_keeps_vertex_counts_for_each_open_polyline` | 通过：2026-08-11 本机 .NET 10，38.DXF 的 81 个开放 POLYLINE 顶点均被保留 |
| P1-DXF-006 | `Header_unit_is_recorded_but_still_requires_business_confirmation` | 通过：2026-08-11 本机 .NET 10，$INSUNITS=4 记录为毫米但仍阻断业务提交 |
| P1-DXF-007 | `ImportDxfUseCaseTests.Import_stays_uncommitted_until_millimetres_are_confirmed` | 通过：2026-08-11；检查不修改项目，确认毫米后记录输入 SHA-256 与导入决定 |
| P1-PRJ-001 | `ProjectStoreTests.Save_then_load_preserves_import_traceability` | 通过：2026-08-11 本机 .NET 10 |
| P1-PRJ-002 | `Save_to_missing_directory_leaves_no_temporary_project_file`、`Saving_an_existing_project_keeps_the_previous_complete_version_as_recovery_copy` | 通过：2026-08-11 本机 .NET 10；写入失败不留下临时文件，替换既有项目时保留上一份完整 `.lnproj.bak` 恢复副本 |
| P1-PRJ-003 | `ProjectDocumentTests.Create_new_project_sets_schema_and_clean_revision` | 通过：2026-08-11 本机 .NET 10 |
| P1-UI-001 | `ImportWizardViewModelTests.Cancel_discards_session...` | 通过：2026-08-11；取消清空会话且不修改项目 |
| P1-UI-002 | `ImportWizardViewModelTests.Workflow_requires_millimetres_confirmation_before_project_is_changed` | 通过：2026-08-11；未确认毫米前项目不变，确认后创建脏修订 |
| P1-PLT-001 | 平台启动与发布 | 部分通过：2026-08-11 Mac arm64 实际启动进程保持运行，跨层 DXF 保存/重开测试通过；已生成 204 MB `win-x64` 自包含发布物。仍需完成 Mac GUI 人工操作与 macOS x64 冒烟；Windows 真机验证移至阶段 6。 |
| P1-E2E-001 | `Stage1WorkflowTests.Real_dxf_can_be_checked_confirmed_saved_and_reopened` | 通过：2026-08-11；真实 DXF 检查、确认毫米、保存与重开保留来源哈希 |

验证命令：

```bash
dotnet test tests/LeatherNesting.Domain.Tests/LeatherNesting.Domain.Tests.csproj -c Release --filter "Stage=1"
dotnet test tests/LeatherNesting.Infrastructure.Tests/LeatherNesting.Infrastructure.Tests.csproj -c Release --filter "Stage=1"
dotnet test LeatherNesting.sln -c Release --filter "Stage=1"
dotnet build LeatherNesting.sln -c Release
```

本次复核：当前源码清单包含 Domain 2 个、Infrastructure 19 个（含 1 个 8 数据行的 Theory）、Desktop 2 个、End-to-End 1 个 Stage 1 测试执行。复核环境未安装 .NET SDK，不能重新确认既有历史结果，也不能运行本次新增的 P1-DXF-004 回归；因此阶段门仍被自动化复验阻断。此前记录的 Python 回归、锁定依赖恢复、整解构建、格式检查和 Windows x64 发布结果仅作为历史证据，不替代本次复验。

尚未完成 P1-PLT-001 的 macOS x64 真机启动/导入/保存冒烟，以及 1366×768、100/125/150% DPI 的人工验收；Windows 真机验证已移至阶段 6，不再作为阶段 1 门的一部分。不得将本记录视为阶段门已通过。

执行步骤与证据格式见 [Stage 1 Manual Smoke Checklist](./stage-1-manual-smoke.md)。
