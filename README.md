# 描述
Unity UGUI Text扩展插件, 支持以下功能:
- Sprite: 图文混排
- Prefab: 预制体混排
- Underline: 文本下划线(使用Image渲染)
- Hyperlink: 超链接(支持内联Sprite等)

> Text扩展功能使用运行时实现, 不支持Editor预览

# Unity版本兼容
| Unity版本 | 是否支持 | 描述 |
| ------------ | ------------ | ------------ |
|  2019.4  | -   | `待测试`  |
|  2020.3  | -   | `待测试`  |
|  2021.3  | √   | 支持  |
|  2022.3  | -   | `待测试`  |

# 如何使用
|  名称 |  标签  | 示例  |
| ------------ | ------------ | ------------ |
| Sprite  |  `<sprite=...>`   |   `<sprite="123456">`  |
| Prefab  |  `<prefab=...>`   |   `<prefab="name">`  |
| Underline  | `<u>...</u>`  |  `<u>内容</u>`   |
| Hyperlink  |  `<link>...</link>`   |  `<link>内容</link>`  |

