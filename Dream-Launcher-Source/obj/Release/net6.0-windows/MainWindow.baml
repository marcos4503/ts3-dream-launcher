   M S B A M L   `   `   `  ���� M  ITS3 Dream Launcher, Version=0.0.1.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TS3_Dream_LauncherTS3_Dream_Launcher  c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  #    TS3_Dream_Launcher.MainWindow   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     U QWpfAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=9e7cd3b544a090dc,gif"http://wpfanimatedgif.codeplex.com  ,local clr-namespace:TS3_Dream_Launcher  IBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton    � Title$  The Sims 3 Dream Launcher�� � SizeToContent$ WidthAndHeight=� � WindowStartupLocation$ CenterScreen=� � Icon$ /Resources/icon-32x.png�� � 
ResizeMode$ CanMinimize=� � 	Resources  �� 
TargetType �� Property �� Value	 �� 
TargetType
 � ColumnDefinitions � RowDefinitions J� Opacity �� VerticalOffset � Value �� ScrollableHeight � Maximum �� ViewportHeight �� ViewportSize* �� #ComputedVerticalScrollBarVisibility �� Orientation �� HorizontalOffset �� ScrollableWidth �� ViewportWidth, �� %ComputedHorizontalScrollBarVisibility   screen1FadeOut �� 
TargetName �� TargetProperty9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value<  6System.Windows.Media.Animation.EasingThicknessKeyFrame p� KeyTime p� Value  screen3FadeIn  screen3FadeOut  screen4FadeIn  screen4FadeOut  screen5FadeIn  screen5FadeOut  screen6FadeIn  
toastEnter 	 	toastExit 
 toastHistoryEnter  toastHistoryExit��%�	  '��       &	  {    &	 �    &	 e    &	 �    &	 \    &	 �    &	 U    &	 �    &	 M    &		     &	
 �    &	 �    �� " ��.���  �@�OverridesDefaultStyle$
 True����  � �� �� "	 ��.R�� 
 �� �� $
��Auto � �� �� $
��Auto �.+��� $��0���� $��PART_VerticalScrollBar��$��1��$	 1.0Q�# z # z # z $��6��$��6��#y�z �� $��PART_HorizontalScrollBar��$	 1.0Q�$ 
Horizontal=�$��1��$��0��$��6��$��6��# z # z # z #y�z �� .��U� $ s1_language��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 1Q�  $ 00:00:00.5000000��$ 1Q�r� $ image��$ (FrameworkElement.Margin)�.	�  $ 00:00:00.5000000��$ 
-264,0,0,0q�r� $ 
stackPanel��$ (FrameworkElement.Margin)�.	�  $ 00:00:00.2000000��$ 	0,128,0,0q�  $ 00:00:00.5000000��$ 0,-1100,0,0q��� .��U� $ 	s3_checks��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 0Q�  $ 00:00:00.5000000��$ 1Q��� .��U� $ 	s3_checks��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 1Q�  $ 00:00:00.5000000��$ 0Q��� .��U� $ s4_translate��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 0Q�  $ 00:00:00.5000000��$ 1Q��� .��U� $ s4_translate��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 1Q�  $ 00:00:00.5000000��$ 0Q��� .��U� $ s5_intelFix��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 0Q�  $ 00:00:00.5000000��$ 1Q��� .��U� $ s5_intelFix��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 1Q�  $ 00:00:00.5000000��$ 0Q��� .��U� $ s6_launcher��$ (UIElement.Opacity)�.\�  $ 00:00:00��$ 0Q�  $ 00:00:00.5000000��$ 1Q��� .��r� $ 
toastPopUp��$ (FrameworkElement.Margin)�.	�  $ 00:00:00��$ 
0,-128,0,0q�  $ 00:00:00.3000000��$ 0,32,0,0q�  $ 00:00:00.5000000��$ 0,8,0,0q��� .��r� $ 
toastPopUp��$ (FrameworkElement.Margin)�.	�  $ 00:00:00��$ 0,8,0,0q�  $ 00:00:00.2000000��$ 0,32,0,0q�  $ 00:00:00.5000000��$ 
0,-128,0,0q��� .��r� $ toastsHistory��$ (FrameworkElement.Margin)�.	�  $ 00:00:00��$ -300,0,0,36q�  $ 00:00:00.2000000��$ 	36,0,0,36q�  $ 00:00:00.3000000��$ 0,0,0,36q��� .��r� $ toastsHistory��$ (FrameworkElement.Margin)�.	�  $ 00:00:00��$ 0,0,0,36q�  $ 00:00:00.1000000��$ 	36,0,0,36q�  $ 00:00:00.3000000��$ -300,0,0,36q�.��� -    �Name$
 root��$
��1208��$	��680��.+�� -   $ s1_language��$y�	Collapsed=�.+��� -     ��Name$  
stackPanel��$	��600��$��Center=�$��Center=�$��0,0,0,0q�.+��� -   ! ��Name$! langSelectTitle��$1��+Please, select a language for the Launcher!��$��Normal"�$��24(�$��Center=�$��Center=�  TheSimsSansSc#���  ���	#FF004E49�� " �� Orientation$" 
Horizontal=�$��Center=�$��Center=�$��0,32,0,0q�.+��� $��80��.+�� $��80��$��80��.+��� -   # ��Name$# langSelectEnUs��$��0,0,0,0q���Z� ��Z� ��Z� .���� $��/Resources/lang-en-us.png���� -   $ ��Name$$ langCheckEnUs��$��0,0,0,0q�$��Right=�$	��Top=�$��32��$��32��$��/Resources/check.png���� $��English��$��Center=�$��26��$��0,0,0,0q�$��Center=��� $��80��$��64,0,0,0q�.+�� $��80��$��80��.+��� -   $# langSelectPtBr��$��0,0,0,0q���Z� ��Z� ��Z� .���� $��/Resources/lang-pt-br.png���� -   $$ langCheckPtBR��$��0,0,0,0q�$��Right=�$	��Top=�$��32��$��32��$��/Resources/check.png���� $��
Português��$��26��$��Center=�$��0,0,0,0q�$��Center=�;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -	   %  Name$% langSave��$
��SAVE��&   HoverBackground& �	#FF00BF0C$	��212��$��34��#���  $��	0,128,0,0q�$��16(����	#FF00A00A���	#FF0F5800
��������� -
   $$ image��$	��373��$	��277��$��/Resources/sim-think.png��$��Bottom=�$
��Left=�$��48,0,0,0q�� -   $ s2_intro��$y�	Collapsed=�.+�r� -   ' r�Name$' introVideoPlayer��$
��Left=�$	��Top=�$
��1208��$	��680��( r� Stretch$
( Fill=�) r� Volume$) 1Q�* r� LoadedBehavior$* Manual=�+ r� UnloadedBehavior$
+ Stop=�� -   $ 	s3_checks��$y�	Collapsed=�.+�� -   $ checking��$y�Visible=�.+��� $��Center=�$��Center=�$" 
Horizontal=�.+��� "  WpfAnimatedGif.ImageBehavior,   AnimatedSource$, /Resources/load.gif��-   AnimationSpeedRatio- 1.5$��36��$��36���� -   $! tt��  screen3_checking#���  #���  $��Center=�$��24(�$��0,0,0,0q����	#FF1E5834� -   $ 
checkError��$y�	Collapsed=�$��Center=�$��Center=�.+��� $��Stretch=�.+��� -   . ��Name$. checkErrorReason��$
��Wrap=�$��Place Holder��$
��Bold"�$��16(�#���  ���	#FF5B0808$��Center=�$��Center=�$
��1128��/ �� TextAlignment$/ Center=�$y�Visible=�  -   $% 
checkClose��  screen3_closeButton#���  & �	#FFB34242$	��212��$��34��#���  $��16(����	#FFA00000���	#FF580000
�������$��Center=�$��Center=�$��0,64,0,0q�� -   $ s4_translate��$y�	Collapsed=�.+��� $��Center=�$��Center=�.+��� $
��Wrap=�  screen4_dialogTitle#���  $��16(�$
��Bold"�#���  $	��800��$/ Center=��� $" 
Horizontal=�$��Center=�$��Center=�$��0,64,0,0q�.+�  -   $% noTranslate��  screen4_dialogN#���  & �	#FFB34242$	��128��$��34��#���  $��16(����	#FFA00000���	#FF580000
�������  -   $% neverTranslate��  screen4_dialogU#���  & �	#FFA0A0A0$��34��#���  $��16(����	#FF6B6B6B���	#FF424242
�������$��32,0,0,0q�$��8,1,8,1q�  -   $% yesTranslate��  screen4_dialogY#���  & �	#FF00BF0C$	��128��$��34��#���  $��16(����	#FF00A00A���	#FF0F5800
�������$��32,0,0,0q�� -   $ s5_intelFix��$y�	Collapsed=�.+�� -   $ intelLoading��$y�Visible=�.+��� $��Center=�$��Center=�$" 
Horizontal=�.+��� $, /Resources/load.gif��- 1.5$��36��$��36����   screen5_Loading#���  #���  $��Center=�$��24(�$��0,0,0,0q����	#FF1E5834�� -   $  intelDialog��$��Center=�$��Center=�$y�	Collapsed=�.+��� -   $. 	intelText��$
��Wrap=�$��16(�$
��Bold"�#���  $	��800��$/ Center=�$��Place Holder���� $" 
Horizontal=�$��Center=�$��Center=�$��0,64,0,0q�.+�  -   $% 
noIntelFix��  screen5_dialogN#���  & �	#FFB34242$	��128��$��34��#���  $��16(����	#FFA00000���	#FF580000
�������  -   $% neverIntelFix��  screen5_dialogU#���  & �	#FFA0A0A0$��34��#���  $��16(����	#FF6B6B6B���	#FF424242
�������$��32,0,0,0q�$��8,1,8,1q�  -   $% yesIntelFix��  screen5_dialogY#���  & �	#FF00BF0C$	��128��$��34��#���  $��16(����	#FF00A00A���	#FF0F5800
�������$��32,0,0,0q�� -   $ s6_launcher��$y�Visible=�.+��� $!��/Resources/neighborhood.png���� $" Vertical=�.+��� $" 
Horizontal=�$��0,0,0,0q�$	��644��.+�� $	��255��$	��644��.+��� $��4,4,4,4q�0 �� RadiusY$0 8��1 �� RadiusX$1 8��$��0������ 2 �� EndPoint$2 0.5,1&�3 �� 
StartPoint$3 0.5,0&�.��� 4 � Color$4 	#FF62C064��� $4 	#FF94C6FF��5 � Offset$5 1Q�� $4 	#FF94C6FF��$
5 0.25Q��� $��16,16,16,16q�.+��� $��/Resources/logo.png��$��Uniform=�6 �� StretchDirection$
6 Both=�$	��250��  -   $% goPlay��  launcher_button_goPlay#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������  -    $% goSaves��  launcher_button_goSaves#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -!   $% goSims��  launcher_button_goSims#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -"   $% goCache��  launcher_button_goCache#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -#   $% 	goPatches��  launcher_button_goPatches#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -$   $% goMods��  launcher_button_goMods#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -%   $% goTools��  launcher_button_goTools#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -&   $% 
goSettings��  launcher_button_goSettings#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -'   $% goAbout��  launcher_button_goAbout#���  & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q�  -(   $% goExit��   launcher_button_goExit#���   & �	#FF5D90C8$��28��#���  $��16(����	#FF2C67A9���	#FF06305F
�������$��0,4,0,0q��� -)   $! gameVersion��$ ��Game Version: 0.0.0.0.0000��$��Center=�$��Center=�$��0,0,0,0q�$��10(�$��0,0,0,8q�$��Bottom=����	#FFF1F1F1� $	��952��$
��Left=�.+��� $��4,4,4,4q�$��0��$1 8��$0 8������ $2 0.5,1&�$3 0.5,0&�.��� $4 	#FFB2D8B2��� $4 	#FFE9F3FF��$5 1Q�� $4 	#FFE9F3FF��$
5 0.25Q��� $��0,0,0,0q�$��0��$��2��7 �� CornerRadius$7 8,8,0,0��
���   ����	#FF4B9C52$	��Top=�$��32��$��4,4,4,0q�._��� -*   $! 	pageTitle��$��Place Holder��$��Center=�$��Center=�#���  
�������$��20(�$
��Bold"�� -+   $ pagesContent��$��4,36,4,4q�.+�� -,   $ pagePlay��$��0,0,0,0q�$y�	Collapsed=�.+��� --   8 ��Name$8 playWallpaper��$��0,0,0,0q�$7 0,0,8,8������ 9 j� Stretch$9 UniformToFill=�: �� ImageSource$ : /Resources/wallpaper-0.jpg��; �� RelativeTransform; V� .~��� < �� CenterY$	< 0.5Q�= �� CenterX$	= 0.5Q��� > �� CenterX$	> 0.5Q�? �� CenterY$	? 0.5Q��� @ �� Angle$@ 0Q�A �� CenterX$	A 0.5Q�B �� CenterY$	B 0.5Q�T� C T� X$C 0Q��� $��0,0,0,0q�$��Bottom=�$	��128��$7 0,0,8,8�����	#B2000000$��0,0,0,0q�._��� -.   $! documentsNotFound�� ! launcher_documentsNotFound#��� ! $	��Top=�$��Center=�$��0,2,0,0q�$��28��$
��Bold"�
�������$y�	Collapsed=�  -/   $% playGame��& �	#FF00BF0C#���  $��36(����	#FF24C82E���	#FF0F5800
�������$��0,0,0,32q�$��Bottom=�$��Center=�$��64��$	��256��.���� $" 
Horizontal=�$��62��.+��� D J� IsHitTestVisible$D False��$��/Resources/play.png��$��28��$��28����  " launcher_button_playButton#��� " #���  $��Normal"�$��0,0,0,0q�$��Center=�$
��Left=�$��12,0,0,0q�
�������� -0   $ 	pageSaves��$y�	Collapsed=�� -1   $ pageSims��$y�	Collapsed=�� -2   $ 	pageCache��$y�	Collapsed=�� -3   $ pagePatches��$y�	Collapsed=�� -4   $ pageMods��$y�	Collapsed=�� -5   $ 	pageTools��$y�	Collapsed=�� -6   $ pageSettings��$y�	Collapsed=�� -7   $ 	pageAbout��$y�	Collapsed=�� $��32��.+��� $��4,0,4,0q����	#FF94C6FF$��0��$1 8��$0 8���� $
��Left=�$��16,0,0,0q�$" 
Horizontal=�.+�� $��32��$��32��$��0,0,16,0q�.+��� -8   E ��Name$E toastsCounterBg��$��16��$��16��$��Right=�$	��Top=�$1 8��$0 8��$��0��
���  ��F K� ZIndex$F 4��$��0,4,0,0q�$y�	Collapsed=��� -9   $! toastsCounterT��$��0��$	��Top=�$��Right=�$��16��$��16��$��	0,3.5,0,0q�$��10(�$��0,0,0,0q�$F 5��$��Center=�$��Center=�
�������$y�	Collapsed=��� -:   $# toggleToastsHistory��$��0,0,0,0q�$��26��$��26����Z� ��Z� ��Z� .���� $��/Resources/bell.png���� -;   $$ doingTasksGif��$, /Resources/task-run.gif��- 1.5$��24��$��24��$��Center=�$��Stretch=�$y�Visible=��� -<   $$ doingTasksOk��$��23��$��23��$y�	Collapsed=�$��/Resources/task-ok.png���� -=   $! tasksStatus��$��Place Holder��$��28��$��Center=�$
��Bold"�$��14(�$��0,0,0,0q�$��8,0,0,0q�
�������$��Center=��� $��32��$��0,0,16,0q�$" 
Horizontal=�$��Right=�.+��� ->   $$ dlc_ep1��$��24��$��24��G ^� InitialShowDelay$
G 1000��H ^� ShowDuration$H 10000��I ^� BetweenShowDelay$I 15000��$��/Resources/dlc_ep1s.png���� -?   $$ dlc_ep2��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep2s.png��$��5,0,0,0q��� -@   $$ dlc_ep3��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep3s.png��$��5,0,0,0q��� -A   $$ dlc_ep4��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep4s.png��$��5,0,0,0q��� -B   $$ dlc_ep5��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep5s.png��$��5,0,0,0q��� -C   $$ dlc_ep6��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep6s.png��$��5,0,0,0q��� -D   $$ dlc_ep7��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep7s.png��$��5,0,0,0q��� -E   $$ dlc_ep8��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep8s.png��$��5,0,0,0q��� -F   $$ dlc_ep9��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep9s.png��$��5,0,0,0q��� -G   $$ dlc_ep10��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep10s.png��$��5,0,0,0q��� -H   $$ dlc_ep11��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_ep11s.png��$��5,0,0,0q��� -I   $$ dlc_sp1��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp1s.png��$��19,0,0,0q��� -J   $$ dlc_sp2��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp2s.png��$��5,0,0,0q��� -K   $$ dlc_sp3��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp3s.png��$��5,0,0,0q��� -L   $$ dlc_sp4��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp4s.png��$��5,0,0,0q��� -M   $$ dlc_sp5��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp5s.png��$��5,0,0,0q��� -N   $$ dlc_sp6��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp6s.png��$��5,0,0,0q��� -O   $$ dlc_sp7��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp7s.png��$��5,0,0,0q��� -P   $$ dlc_sp8��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp8s.png��$��5,0,0,0q��� -Q   $$ dlc_sp9��$��24��$��24��$
G 1000��$H 10000��$I 15000��$��/Resources/dlc_sp9s.png��$��5,0,0,0q�� -R   $ 
toastPopUp��$	��Top=�$��0,8,0,0q�$��Center=�$	��800��$F 5��$y�	Collapsed=�.+��� -S   $E toastBg��$��2�����	#E5002E88$��4,0,4,0q�$1 8��$0 8�����	#FF000746�� -T   $. toastMessage��$
��Wrap=�$��	TextBlock��$
��Bold"�$��14(�$
��Left=�$��16,8,0,8q�$	��650��
�������  -U   $% toastDismiss�� # launcher_toast_dismiss#��� # & �	#FF7AB2F3#���  ���	#FF558FD2���	#FF11355F
�������$��0,0,16,0q�$��Right=�$��Center=�$��24��$��8,0,8,0q�� -V   $ toastsHistory��$
��Left=�$	��255��$��0,0,0,36q�$y�	Collapsed=�.+��� $��4,4,4,4q����	#FF94C6FF$��0��$1 8��$0 8���� $��0,0,0,0q�$	��Top=�$7 8,8,0,0��$��32��$��4,4,4,0q����	#FF4B9C52._���  $ launcher_toastsHistoryTitle#��� $ $��Center=�$��Center=�#���  
�������$��20(�$
��Bold"�$��0,0,0,0q��� -W   J ��Name$J toastHistoryScroll��$��4,36,4,4q�.���� -X   $  toastsHistoryContainer��$	��Top=�.+�� -Y   $ toastHistEmpty��$	��108��$��0,16,0,0q�.+��� $��80��$��80��$	��Top=�$��/Resources/ghost.png��$��UniformToFill=�$	 0.5Q���  % launcher_toastsHistoryEmpty#��� % $��Bottom=�$��Center=�
�������$��Normal"�$
 0.75Q�$��Normal$�