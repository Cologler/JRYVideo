# JRYVideo

[![Join the chat at https://gitter.im/Cologler/JRYVideo](https://badges.gitter.im/Cologler/JRYVideo.svg)](https://gitter.im/Cologler/JRYVideo?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

JRYVideo a video tracker

## 怎么编译

要求 VS2015 及以上。

数据库用的接口形式，但是我只实现了 mongodb 接口，而且很懒惰地把数据库账号和密码写死在代码里（JryVideoMongoDbDataEngine.cs），有爱自改。

需要打开 jasily.sln 编译一次，才能在 jryvideo.sln 里面编译，因为 jasily 引用了第三方 nuget 包。

## 这货大概这么个样子

一打开程序，显示正在追的剧(tracking)：

![main-tracking](http://i.imgur.com/VUnI9if.png)

左上角有个 tracking，取消选择就会变成不是正在追的：

![main-untrack](http://i.imgur.com/IL9Nywh.png)

存档没有追剧的分组和排序。

我的建议是将还没播出的未来剧（比如两年后的好莱坞大片？）设置为 tracking，反正排序它都会排在最底部，而且到播出时间了会自动推到顶部。

左下角是内存占用，还算可以接受。

## tracking

从截图上可以看到今天我打算看三部小日番。

今天会播出的番右上角会有橙色的标签，done 表示已经播出完了，但是我还没看完，所以没将其追剧设置为否。
（实际上我在考虑着将播出完毕的单独分一组。）

排序的话，同组内优先没看的，然后才是看过的。

看完一部，可以在 video 界面勾选该集。

## add video

先介绍下 series-video 模式，
比如柯南，就是一个系列（series），柯南里面某部剧场版，应该对应一个 video；
比如权力的游戏，就是一个系列（series），已经出了 n 季，应该对应 n 个 video；

点击 main 左上角的 add 按钮：

#### 1. select series

先选择 series，比如柯南，可以搜索：

左侧选择右侧添加，搜索不到再添加，每行一个名字，支持从豆瓣载入，填豆瓣电影的 id 就好：

![add-video-1](http://i.imgur.com/LxmkBOY.png)

选择好了点 next step。

#### 2.

![add-video-2](http://i.imgur.com/LxmkBOY.png)

确保 video 不存在。series 和 video 现在只能从数据库删除，会比较麻烦（我暂时没有删除的需求）。

没有图片说明我还没给它添加封面 = =。

next step。

#### 3. add video

![add-video-3](http://i.imgur.com/L64VmYA.png)

开始填数据...

有几点说下：

* douban id: 部分数据可以通过豆瓣电影载入，填入 id 点 load 按钮就好。包括封面也可以。
* offset: 比如美剧冬歇会导致计算本周第几集出错，所以添加个偏移量。
* start date: 始播时间
* day of week: 一般美剧一天后这边才出熟肉，所以添加一个偏移量，这里我一般设置为 start date 的第二天。
* name: 比如柯南剧场版都有个子名，欢迎填入。

填好数据后点 commit 就好了。出错会有提示。

我的类型有这么多，你可以随便添加：

![video-type](http://i.imgur.com/VxXI25P.png)

## video

![video-main](http://i.imgur.com/iNR2jkc.png)

底部是看了的部分。像柯南那种 800 多集，UI 绘制会变卡（捂脸

那么那个 add 和一大块空白是什么鬼？我就放个图，不说话：

![video-main-with-add](http://i.imgur.com/DwU6hUu.png)

## imdb

填入 imdb 自动生成背景，美爆了：

![video-main-bg1](http://i.imgur.com/IJ1mTFf.png)

![video-main-bg2](http://i.imgur.com/E29SZb2.png)

![video-main-bg3](http://i.imgur.com/DPguV1W.png)

## 多语言

支持中文或英文，部分没有中文（因为我先写的英文），不支持选择语言，目前依赖系统设置。
