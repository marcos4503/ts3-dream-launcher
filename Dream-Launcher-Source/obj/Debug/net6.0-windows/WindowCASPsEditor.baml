   M S B A M L   `   `   `  ����M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TS3_Dream_LauncherTS3_Dream_Launcher  5   	   c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  5   	   *    $TS3_Dream_Launcher.WindowCASPsEditor   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:TS3_Dream_Launcher     5   	   U QWpfAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=9e7cd3b544a090dc,gif"http://wpfanimatedgif.codeplex.com  5   	   IBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton  5   	     � Title$#  Dream Launcher - CASPs Editor��5
   	    � WindowStyle$ 
ToolWindow=�6/    � Icon$ /Resources/icon-32x.png��6H    � 
ResizeMode$ NoResize=�6g    � SizeToContent$ WidthAndHeight=�6}    � WindowStartupLocation$ CenterScreen=�6�    � ShowInTaskbar$ False��6�    � Topmost$
 True��6�   .��5      � $	��800��6   $	��600��6   .+�5   
   � $��0,0,0,44q�6   .+�5      �� -   5         ��Name$ 
loadingGif��5      "  WpfAnimatedGif.ImageBehavior	   AnimatedSource$	 Resources/load.gif��6(   
   AnimationSpeedRatio
 1.56^   $��Center=�6�   $��Center=�6�   $��48��6�   $��48��6�   $y�	Collapsed=�6�   $��/Resources/load.gif��6�   �� 5      -   5         ��Name$ contentScroll��5      .��5      � $	��Top=�6   $��8,8,8,8q�6/   .+�5      �� -   5         ��Name$ 	emptyWarn��5      $��Place Holder��6/   $��Center=�6F   $	��Top=�6c   $��0,0,0,0q�6{   $��0,8,0,0q�6�   ���	#FFA5A5A56�   � 5      -   5         �Name$ contentList��5       $	��Top=�65   $��Center=�6M   5      5      �� 5      $	��0.5��6   
���   �$��Bottom=�64   5      ;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  5   
   -   5          Name$ 
saveButton��5   $      HoverBackground �	#FF5D90C8   TheSimsSansSc#���   6T   ���	#FF2C67A9���	#FF06305F
������� � Cursor$
 Hand��6�   $��0,0,0,8q�6�   $��Bottom=�6�   $��Center=�6�   $��28��6  $	��200��6&  $��PLACE HOLDER��62  $��16(�6I  �� 5   
   -   5        $ 	savingGif��5      $"	 Resources/load-spinner-0.gif��6#   
 1.56c   $y�	Collapsed=�6�   $
��Fill=�6�   $��0,0,0,8q�6�   $��Bottom=�6�   $��Center=�6�   $��28��6�   $��28��6
  $#��/Resources/load-spinner-0.gif��6  5      5      