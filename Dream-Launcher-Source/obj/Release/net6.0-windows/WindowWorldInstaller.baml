   M S B A M L   `   `   `  ���� M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TS3_Dream_LauncherTS3_Dream_Launcher  k:clr-namespace:TS3_Dream_Launcher.Controls.ValidableTextBox,TS3_Dream_Launcher.Controls.ValidableTextBox  c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  -    'TS3_Dream_Launcher.WindowWorldInstaller   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:TS3_Dream_Launcher     WValidableTextBox:clr-namespace:TS3_Dream_Launcher.Controls.ValidableTextBox     OBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton     U QWpfAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=9e7cd3b544a090dc,gif"http://wpfanimatedgif.codeplex.com    � Title$  Place Holder�� � WindowStyle$ 
ToolWindow=� � Icon$ /Resources/icon-32x.png�� � 
ResizeMode$ NoResize=� � ShowInTaskbar$ False�� � SizeToContent$ WidthAndHeight=� � Topmost$
 True�� � WindowStartupLocation$ CenterScreen=� � 	Resources    extractionExit	 �� 
TargetName
 �� TargetProperty9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value  installEnter��%  &	        &	 }     �� .��U� $	 extractionUi��$
 (UIElement.Opacity)�.\�  $ 00:00:00��$ 1Q�  $ 00:00:00.3000000��$ 0Q��� .��U� $	 	installUi��$
 (UIElement.Opacity)�.\�  $ 00:00:00��$ 0Q�  $ 00:00:00.3000000��$ 1Q�.��� $	��512��$	��600��.+�� -    �Name$ extractionUi��$y�	Collapsed=�.+��� $��Center=�$��Center=�.+��� -    ��Name$ extractTitle��$
��Wrap=�$��	TextBlock��$	��Top=�$��Center=�� $��12��$	��128��$	��Top=�$��0,8,0,0q� � IsIndeterminate$
 True��� -   $ 	installUi��.+��� -    ��Name$ content��$��8,8,8,0q�$	��Top=�.+��� -   $ 	titleMeta��$
��Wrap=�$��Title��$��20(�$
��Bold"�  TheSimsSansSc#���  $��Center=�$��0,0,0,16q��  � ColumnDefinitions �� $	��128 ��� $��16 ��� .+�� $	��128��$	��128��.+��� -    ��Name$ thumb��$��0,0,0,0q� �� CornerRadius$ 16,16,48,16������  j� Stretch$ UniformToFill=� �� ImageSource$  /Resources/world-thumb.png���� -   $ 	pickThumb��$��0,0,0,0q�$��36��$��36��$��Bottom=�$��Right=�
�������$ 16,0,0,0�� � Cursor$
 Hand��._��� $��/Resources/thumb-pick.png��$��Center=�$��Center=�$��20��$��20�� J� Opacity$	 0.5Q�C   =TS3_Dream_Launcher.Controls.ValidableTextBox.ValidableTextBox  -     Name$
 name��$��2��   	LabelName$ Place Holder��� $��25��$��	0,16,0,16q�$	��Top=�.+��� -	   $ 
titleFiles��$
��Wrap=�$��Title��$��20(�$
��Bold"�#���  $��0,0,0,0q�$��Center=�$��Center=��� -
    ��Name$ 	atachFile��$��0,0,0,0q�$��Right=�$��Center=�$��20��$��20��$
 Hand��$��0,0,0,0q���Z� ��Z� ��Z� .���� $��Stretch=�$��Stretch=�$��/Resources/atach-icon.png��$��1,1,1,1q�$	 0.7Q��� ���	#FFB3ABAB$��1q�$	��200��._��� .��� $	��Top=�$��8,8,8,8q�.+��� -   $ 	filesList��$	��Top=�� $	��Top=�$��0,8,0,0q� �� $��24 ��� $��8 ��� .+��� $��/Resources/tip-lamp.png��$
 0.75Q�$	��Top=��� -   $ tipText��$
��Wrap=�$��Place Holder��$��0,0,0,0q�$	��Top=�$	 0.7Q�$��3��$��2���� $	��0.5��
���   �$��Bottom=�$��0,0,0,44q�;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -     Name$ installButton��   HoverBackground �	#FF5D90C8#���  ���	#FF2C67A9���	#FF06305F
�������$
 Hand��$��0,0,0,8q�$��Bottom=�$��Center=�$��28��$	��200��$��PLACE HOLDER��$��16(��� -    ��Name$ installingGif��"  WpfAnimatedGif.ImageBehavior   AnimatedSource$# /Resources/load-spinner-0.gif��   AnimationSpeedRatio 1.5$y�	Collapsed=�$
��Fill=�$��0,0,0,8q�$��Bottom=�$��Center=�$��28��$��28��$#��/Resources/load-spinner-0.gif��