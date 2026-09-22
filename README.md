# Ghost Transfer Protocol

## 《请神协议：新港市镇煞》

EvoTavern 进化酒馆黑客松赛道二：NEW LIFE｜AI 游戏与交互世界。

本项目是一款 Unity 驱动的东方赛博神社体感战斗 Demo：玩家通过手势完成镇煞仪式，Ghost 会记住玩家的选择、转移义体，并改变下一位玩家进入的世界。

## 当前主线

- 团队主仓库：<https://github.com/zackyao02/Ghost-Transfer-Protocol-.git>
- 团队分支：`master`
- Unity 目标目录：`unity/ai-project/YaowuDemo`
- 3D 资产：Tripo 生成山魈、法相、石精和 Ghost 义体
- 手势输入：Python MediaPipe → 本地 JSON → Unity
- 历史源码来源：<https://gitee.com/Zack02/please-god.git>，仅用于迁移，不再作为团队开发入口

## 项目目标

- 一个赛博神社场景
- 一段 120～150 秒完整体验
- 剑诀、净化符、神降三种技能
- 石精敌群与山魈 Boss
- 一个 World Director Agent
- Ghost 记忆和结局持久化
- 摄像头失败时仍可用键盘导演模式完成演示

## 文档

完整 PRD、GDD、技术架构、Tripo 资产规范、三人分工、里程碑和路演预案位于 [`docs/`](./docs/)。

## 开发原则

> 可运行 > 可演示 > 有记忆点 > 系统完整

- 不做开放世界、多地图、多 Boss、多 Agent。
- 不让实时生成、网络或摄像头成为通关前提。
- `master` 始终保持可运行。
- 日常集成使用 `integration/demo`。

## 分支

```text
master                 稳定版本
integration/demo       每日集成
feature/scene          场景与关卡
feature/characters     角色、Tripo、VFX、音效
feature/input          玩法、手势、UI、Agent、构建
```
