   M S B A M L   `   `   `  ����M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null]3clr-namespace:TS3_Dream_Launcher.Controls.ListItems%TS3_Dream_Launcher.Controls.ListItems  5      c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  5      5    /TS3_Dream_Launcher.Controls.ListItems.PatchItem   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Elocal3clr-namespace:TS3_Dream_Launcher.Controls.ListItems     5      OBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton     5      U QWpfAnimatedGif, Version=2.0.0.0, Culture=neutral, PublicKeyToken=9e7cd3b544a090dc,gif"http://wpfanimatedgif.codeplex.com  6Z   .��5
      � .+�5   
   �� ���	#FFC8C8C8$��1,1,1,1q�6)   $	��Top=�6C     �� CornerRadius$  8,8,8,8��6[   ._�5      � $��8,8,8,8q�6    � ColumnDefinitions 5      �� 5      $��64 �6'   �� 5      $	��50* �6'   �� 5      $	��100 �6'   .+�5      �� $	��Top=�6   $��0,0,0,0q�65   $��64��6F   $
��Left=�6Q   .+�5      � $��48��6   $��48��6'   $��Center=�62   .+�5      �� -   5         ��Name$
 icon��5   !   
���   �$��1q�6C   $  48,48,48,48��6W   �� 5      -   5        $ 
iconStatus��5   !   $��0,0,0,0q�65   $  48,48,48,48��6O   $��16��6j   $��16��6u   $��Bottom=�6�   $��Right=�6�   
���   �6�   5      �� 5      -   5         ��Name$ status��5       $
��Wrap=�60   $��Status��6D   $	��Top=�6R   $��Center=�6j   $��0,4,0,0q�6�   $��10(�6�    J� Opacity$
 0.75Q�6�    �� TextAlignment$ Center=�6�   5      �� 5      $	��Top=�6   $��8,0,8,0q�65   $
��Left=�6F   $��1��6a   .+�5      �� -   5        $ title��5       $
��Wrap=�6/   $��Title��6C   $��0,0,0,0q�6P   $��Normal"�6a   $��Stretch=�6u   $��Stretch=�6�   $��18(�6�   �� 5      -   5        $ description��5       $
��Wrap=�65   $��Description��6I   $��0,8,0,0q�6\   ���	#FF4646466m   5      �� 5      $	��100��6   $	��Top=�6)   $
��Left=�6A   $��2��6\   $��0,20,0,0q�6l   .+�5      ;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -   5          Name$ installButton��5   0      launcher_patches_install#���   6G      HoverBackground �	#FF5D90C8  TheSimsSansSc#���  6�   ���	#FF2C67A9���	#FF06305F
������� � Cursor$
 Hand��6  $��24��6    5      -   5        $ reInstallButton��5   0     launcher_patches_reInstall#���  6I    �	#FF3C3C3C#���  6�   ���	#FF191919
���   �
�������$
 Hand��6  $��24��6  $y�Visible=�6!    5       -   5        $ 	noActions��5    0     launcher_patches_noActions#���  6C    �	#FF5D90C8#���  6�   ���	#FF2C67A9���	#FF06305F
�������$
 Hand��6  $��24��6  ${�False��6  $y�Visible=�61  �� 5!      -	   5        	 ��Name$	 installingGif��5!      "  WpfAnimatedGif.ImageBehavior
   AnimatedSource$#
 /Resources/load-spinner-0.gif��63      AnimationSpeedRatio 1.56t   $��24��6�   $��24��6�   $y�Visible=�6�   $��0,0,0,0q�6�   $	��Top=�6�   $
��Fill=�6�   5"      5#      5$      5&      5'      