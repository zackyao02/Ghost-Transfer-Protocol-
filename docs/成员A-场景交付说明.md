# 成员 A 场景交付说明

## 当前交付

feature/scene 运行 SampleScene 后，由 ShrineSceneLayout 创建 Scene_A_CyberShrine：

- 废弃赛博神社灰盒：庭院、参道、祭坛、神龛、Boss 鸟居入口。
- 环境表现：青金/朱红灯光、指数雾、雨、城市远景。
- 五个固定镜头节点。
- 两个石精点位、一个山魈点位、一个神降位置、一个结局焦点。
- 封存、共生、换壳三种结局的场景伤痕占位。

## 场景锚点

| 名称 | 类型 | 幕 | 用途 |
|---|---|---:|---|
| CameraNode_Act1_Calibration | Camera | 1 | 神龛校准 |
| CameraNode_Act2_Courtyard | Camera | 2 | 污染庭院 |
| CameraNode_Act3_MemoryAltar | Camera | 3 | 记忆祭坛 |
| CameraNode_Act4_Boss | Camera | 4 | 山魈 Boss |
| CameraNode_Act5_Deity | Camera | 5 | 神降结局 |
| Spawn_Stone_Left | StoneSpawn | 2 | 左石精 |
| Spawn_Stone_Right | StoneSpawn | 2 | 右石精 |
| Spawn_Boss_Mandrill | BossSpawn | 4 | 山魈入口 |
| Reveal_Deity | DeityReveal | 5 | 法相显现 |
| Focus_EndingChoice | EndingFocus | 5 | 三结局符印 |

所有点位都挂有 SceneAnchor，成员 C 可按 AnchorType、ActIndex 或 AnchorId 查询，不需要硬编码坐标。

## 流程接入接口

成员 C 获取 ShrineEnvironmentController 后，可调用 ApplyActVisual(1～5) 切换五幕灯光、雾和雨，并调用 ApplyEndingVisual 选择 Seal、Coexist 或 Transfer 的结局伤痕。

成员 C 只需在流程状态切换时调用接口；成员 B 将角色与 VFX Prefab 对齐对应锚点即可。场景模块不处理手势、技能伤害、Agent 或角色逻辑。

## 当前限制

- 当前机器未安装 Unity Editor，尚未完成真实编译和画面验收。
- 现阶段为程序化灰盒；B 的正式 Prefab 到位后由 A 在主场景替换占位表现。
- 旧 Demo 的 BuildTempleEnvironment 暂时保留，便于迁移基线回退；联调确认后再清理重复占位。