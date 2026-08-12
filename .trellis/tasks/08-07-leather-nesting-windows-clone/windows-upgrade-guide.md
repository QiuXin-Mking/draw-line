# 工厂 Windows 升级与排样软件兼容操作指南

> 适用任务：`08-07-leather-nesting-windows-clone`  
> 状态：规划版，正式发布前必须在非生产试点机上演练并补充截图

## 1. 兼容边界

| 系统 | 等级 | 处理方式 |
|---|---|---|
| 微软仍支持且发布矩阵已验证的 Windows 11 x64 | 正式支持 | 提供标准安装包，记录实际版本号并执行全量冒烟测试 |
| Windows 10 Enterprise/IoT LTSC 2019、2021 x64 | 正式支持 | 分别建立测试环境，验证安装、DXF 导入、排样和导出 |
| Windows 10 Home/Pro 22H2 x64 | 尽力兼容 | 可执行冒烟测试，但明确提示操作系统已结束免费支持 |
| Windows 7、8.1、XP、Vista | 不支持 | 执行本指南的升级或设备替换流程 |
| Windows x86 32 位 | 暂不交付 | 现场盘点确认足够需求后再立项验证 |

## 2. 升级前设备盘点

在目标电脑上完成并记录：

1. `Win + R` 运行 `winver`，记录 Windows 版本、版本号、OS Build 和 Home/Pro/Enterprise/LTSC/IoT 版本。
2. `Win + R` 运行 `msinfo32`，记录系统类型、CPU、内存、BIOS 模式和安全启动状态。
3. `Win + R` 运行 `tpm.msc`，记录 TPM 是否存在及规范版本。
4. 记录系统盘和数据盘的剩余空间。
5. 记录加密狗、切割机、投影仪、打印机、串口卡、PCI/USB 控制卡及其驱动版本。
6. 记录 AutoCAD、切割控制软件、现有排样软件的版本和许可证恢复方式。

## 3. 升级前备份

必须同时准备“系统回退”和“业务数据恢复”：

- 使用可验证的工具制作完整系统盘镜像。
- 单独复制 DXF、订单、排样结果、参数库和程序配置。
- 保存软件安装包、许可证、激活/解绑信息和加密狗驱动。
- 导出或备份设备驱动，记录 IP 地址、共享目录、串口号、波特率和设备参数。
- 制作可启动的恢复介质，并实际确认试点机能识别该介质。
- 在开始升级前随机抽查备份文件，确认能够读取。

## 4. 升级路线

### 4.1 Windows 10 且符合 Windows 11 要求

1. 安装并运行微软 PC Health Check，确认 CPU、TPM 2.0、安全启动、内存和存储检查通过。
2. 向外设/控制软件厂商确认 Windows 11 驱动和软件版本。
3. 在非生产时段打开“设置 → 更新和安全 → Windows 更新 → 检查更新”。
4. 只在 Windows Update 明确提供升级时执行，遵守兼容性保护暂停，不强行跳过。
5. 完成升级后执行第 5 节的全量验收。

### 4.2 Windows 7/8.1 或无法直接升级

1. 先核对硬件是否满足 Windows 11 官方要求，以及所有生产外设是否有 Windows 11 驱动。
2. 使用微软官方安装介质和合法许可证执行全新安装。
3. 按顺序恢复主板/芯片组与网络驱动、生产外设驱动、CAD/控制软件、许可证及业务数据。
4. 完成第 5 节验收前，原生产机不停用、不重置、不删除备份。

### 4.3 硬件不满足 Windows 11 要求

- 不绕过 CPU、TPM 2.0 或安全启动限制。
- 优先更换主机，或保留原设备控制电脑，增加一台受支持的排样工作站，通过 DXF 文件交换。
- 若必须采用 Windows Enterprise/IoT LTSC，应通过设备厂商或正规授权渠道获取，不使用不明来源的系统镜像。

## 5. 升级后业务验收

按顺序记录“通过/失败/不适用”：

1. Windows 激活、系统时间、时区、网络、共享目录和权限。
2. 加密狗、打印机、串口、切割机、投影仪和控制卡。
3. AutoCAD 及原有生产软件能否启动、激活、读取历史数据。
4. 排样软件的安装、启动、DXF 导入、自动排样、手动微调、DXF/PNG/报告导出。
5. 将导出 DXF 在 AutoCAD 中打开，核对单位、尺寸、图层、轮廓和数量。
6. 用一个非关键订单连续运行一个完整班次，记录崩溃、卡顿和外设断连。

## 6. 失败回退

出现以下任一情况应停止投产并回退：

- 关键外设没有稳定驱动。
- 原有控制软件或许可证无法恢复。
- DXF 尺寸、单位或导出结果与升级前不一致。
- 排样、切割或投影过程出现稳定性问题。

回退时使用升级前已验证的系统镜像，恢复后重复验证网络、外设、许可证和一个历史订单。

## 7. 官方参考

- [.NET 10 支持的 Windows 版本](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md)
- [Windows 11 系统要求](https://support.microsoft.com/en-us/windows/windows-11-system-requirements-86c11283-ea52-4782-9efd-7674389a7ba3)
- [PC Health Check 使用说明](https://support.microsoft.com/en-us/windows/experience/compatibility/how-to-use-the-pc-health-check-app)
- [Windows 安装介质与重新安装](https://support.microsoft.com/en-US/Windows/deployment/install-upgrade/reinstall-windows-with-the-installation-media)
