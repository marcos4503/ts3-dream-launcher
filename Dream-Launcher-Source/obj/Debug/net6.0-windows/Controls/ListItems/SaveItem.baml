   M S B A M L   `   `   `  ����M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null]3clr-namespace:TS3_Dream_Launcher.Controls.ListItems%TS3_Dream_Launcher.Controls.ListItems  5      4    .TS3_Dream_Launcher.Controls.ListItems.SaveItem   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Elocal3clr-namespace:TS3_Dream_Launcher.Controls.ListItems     5      .��5	      � $	��Top=�6   .+�5
   
   �� -   5          ��Name$  
background��5
      ���	#FFC8C8C8$��1q�6=   $��Center=�6Q    �� CornerRadius$ 8,8,8,8��6l   ._�5      � $��8,8,8,8q�6   $	��Top=�6$    � ColumnDefinitions 5      �� 5      $	��108 �6'   �� 5      $��8 �6'   �� 5      .+�5      � $	��108��6   $	��108��6$   .+�5      �� $��0,0,0,0q�6   $ 8,8,8,8��67   ��5      �� 5       j� Stretch$ UniformToFill=�6)    �� ImageSource$' !/Resources/portrait-bg-bitmap.png��6A   5      �� 5      -   5         ��Name$ portrait��5      5      � 5      $��2��6   .+�5      �� $	��Top=�6!   $��0,0,0,0q�69   .+�5      �� -   5         ��Name$
 name��5       $��Save Game Name��6.   $��Center=�6G   $
��Left=�6b   $��0,0,0,0q�6}   $��Center=�6�   $��18(�6�   $��Normal"�6�   �� 5       �� Orientation$ 
Horizontal=�6%   $��0,8,0,0q�6>   $	��Top=�6O   $
��Left=�6g   .+�5      �� $��12��6$   $��12��60   $��Center=�6;   $
��Left=�6V   $%��/Resources/wfile-type-world.png��6q    J� Opacity$	 0.5Q�6�   	 � Cursor$
	 Help��6�    "  launcher_save_saveWorldTooltip
 � ToolTip#
 �   6�   �� 5      -   5        $ 	saveWorld��5   $   $��Place Holder��67   $��Center=�6N   $��0,0,0,0q�6p   ���	#FF7F7F7F$��Center=�6�   $
��Left=�6�   $��6,0,0,0q�6�   5      �� 5       $	��Top=�6%   $
��Left=�6=   $��0,4,0,0q�6X   $ 
Horizontal=�6i   .+�5!      �� $��12��6$   $��12��60   $��Center=�6;   $
��Left=�6V   $#��/Resources/save-last-edit.png��6q   $	 0.5Q�6�   $
	 Help��6�    ! launcher_save_lastEditTooltip#
 �  6�   �� 5"      -   5        $ lastEdit��5"   $   $��Place Holder��66   $��Center=�6M   $��0,0,0,0q�6o   ���	#FF7F7F7F$��Center=�6�   $
��Left=�6�   $��6,0,0,0q�6�   5#      �� 5$      $ 
Horizontal=�6%   $��0,4,0,0q�6>   $
��Left=�6O   $	��Top=�6j   .+�5%      �� $��12��6$   $��12��60   $��Center=�6;   $
��Left=�6V   $��/Resources/save-vault.png��6q   $	 0.5Q�6�   $
	 Help��6�    " launcher_save_lastVaultTooltip#
 �  6�   �� 5&      -   5        $ 	lastVault��5&   $   $��Place Holder��67   $��Center=�6N   $��0,0,0,0q�6p   ���	#FF7F7F7F$��Center=�6�   $
��Left=�6�   $��6,0,0,0q�6�   5'      �� 5(      $ 
Horizontal=�6%   $��0,4,0,0q�6>   $
��Left=�6O   $	��Top=�6j   .+�5)      �� $��12��6$   $��12��60   $��Center=�6;   $
��Left=�6V   $��/Resources/media-info.png��6q   $	 0.5Q�6�   $
	 Help��6�    " launcher_save_totalSizeTooltip#
 �  6�   �� 5*      -   5        $
 size��5*   $   $��Place Holder��62   $��Center=�6I   $��0,0,0,0q�6k   ���	#FF7F7F7F$��Center=�6�   $
��Left=�6�   $��6,0,0,0q�6�   5+      5,      5-      �� 5.      -   5        $ badWarn��5.      $��3��6)   $��20��6=   $��20��6I   $��Right=�6T   $	��Top=�6p   $��0,2,32,0q�6�   $��/Resources/save-bad.png��6�   $
	 Help��6�     launcher_save_badGameTip#
 �  6�   �� 5/      -	   5         ��Name$ 
moreButton��5/      $��2��6-   $��0,0,0,0q�6�   $��Right=�6�   $	��Top=�6�   $��24��6�   $��24��6�   $
	 Hand��6�   $��0,0,0,0q�6�   ��6=   Z� ��6S   Z� ��6j   Z� .��50      �� $��Stretch=�6   $��Stretch=�6:   $$��/Resources/three-dots-icon.png��6V   $��2,2,2,2q�6~   51      52      53      55      56      