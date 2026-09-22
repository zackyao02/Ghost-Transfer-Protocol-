# 成员 A 场景交付说明

## 当前交付

feature/scene 运行 SampleScene 后，由 ShrineSceneLayout 创建 Scene_A_CyberShrine：

- 废弃赛博神社灰盒：石板参道、符文法阵、祭坛、层叠屋檐、两侧小神龛和 Boss 鸟居入口。
- 环境表现：青金/朱红灯光、指数雾、雨、带窗光的城市远景和神社背后的 Ghost 符环。
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

## 查看效果

在 Unity 打开 Assets/Scenes/SampleScene.unity，点击 Play，再切到 Game 标签查看玩家镜头。Scene 标签中的网格、彩色线框和灯光图标是编辑辅助线；需要时关闭右上角 Gizmos。

## 当前限制

- 已用 Unity 2022.3.62f3c1 在独立副本中完成脚本编译和第一幕、第五幕静态玩家镜头渲染；仍需在正式工程中试玩确认性能和玩法。
- 现阶段为程序化场景美术第一版；B 的正式 Prefab 到位后由 A 在主场景替换占位表现。
- 旧 Demo 的 BuildTempleEnvironment 方法暂时保留供对照，但启动流程已停止调用，避免双层地面和重复祭坛。
## 场景美术化第一版

- 参道与庭院石材使用 `Assets/Resources/SceneTextures/BasaltPaving.png`，由内置 imagegen 工具生成，提示词目标是“无文字、无光照烘焙、可重复的冷色磨损玄武岩地砖”。
- 入口补鸟居框景与悬挂符纸；神社补坡屋顶、瓦棱、木格栅与祭坛机械浮雕；庭院补雨水池、破损石材和墙体节奏。
- 第一至第五幕各有独立视觉层：校准环、污染裂口、记忆数据列、Boss 地面预警、神降金色法阵。第四幕会收起记忆碑，露出神社后方的 Boss 门洞；仍通过原有 `ApplyActVisual` 接口切换。
- 增加局部补光与无需新包的轻量镜头色调 Shader；不修改手势、战斗或角色资产。
- 独立 QA 截图位于 `work/scene-qa/player-view.png`、`work/scene-qa/act4-view.png` 与 `work/scene-qa/act5-view.png`（该目录被 Git 忽略，正式预览请在 Unity 的 Game 标签查看）。