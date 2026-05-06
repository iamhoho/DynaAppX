---
name: DynaAppX Project Status
description: DynaAppX XrmToolBox WPF migration project current status
type: project
---

# DynaAppX 项目状态

## 项目结构
- `~/repos/DynaAppX` - GitHub: https://github.com/iamhoho/DynaAppX
- 远程已配置为 SSH: git@github.com:iamhoho/DynaAppX.git

## 分支
- `main-XrmToolBox-wpf` - WPF 版本开发分支（当前工作分支）
- `main-XrmToolBox` - Vue 技术栈原始版本分支

## 当前进度
已迁移 AccessCheck 功能到 WPF，正在测试调试阶段。

## 待完成功能
1. AccessCheck - 用户权限检查（基本完成，需调试）
2. InvokeFlow - 调用工作流/Action（未迁移）
3. GodPage - 实体记录编辑器（未迁移）
4. MetadataBrowser - 跳转外部页面（简单，可后续实现）

## 技术要点（重要）
1. **迁移方向**: Vue → WPF，原有 vue-project 是参考实现
2. **WPF + XrmToolBox**: 使用 ElementHost 托管 WPF UserControl
3. **EntityMetadata**: 必须用 `RetrieveAllEntitiesRequest`，不能用 FetchXML 查询 `entitydefinition`
4. **FetchXML 返回值**: 属性存在 Entity 的字典里，不是直接属性。需要包装类（UserWrapper, EntityWrapper, RoleWrapper, TeamWrapper, RecordWrapper）
5. **缓存**: 元数据查询较慢，需要缓存 `_allEntities`
6. **筛选**: 控件下拉框打开时从缓存筛选，不是每次请求 API

## 下次继续
- 拉取最新代码测试 AccessCheck
- 如果还有 bug 继续修复
- 准备迁移 InvokeFlow 功能
