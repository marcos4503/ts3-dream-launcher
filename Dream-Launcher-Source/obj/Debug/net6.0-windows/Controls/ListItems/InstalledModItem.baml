   M S B A M L   `   `   `  ����M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null]3clr-namespace:TS3_Dream_Launcher.Controls.ListItems%TS3_Dream_Launcher.Controls.ListItems  5      c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  5      <    6TS3_Dream_Launcher.Controls.ListItems.InstalledModItem   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Elocal3clr-namespace:TS3_Dream_Launcher.Controls.ListItems     5      OBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton     5      .��5
      � .+�5   
   �� ���	#FFC8C8C8$��1,1,1,1q�6)   $	��Top=�6C     �� CornerRadius$  4,4,4,4��6[   ._�5      � $��4,4,4,4q�6    � ColumnDefinitions 5      �� 5      $��24 �6'   �� 5      $��* �6'   �� 5      $	��100 �6'   �� 5      $��8 �6'   �� 5      $��24 �6'   .+�5      �� -   5         ��Name$ enabled��5      $��0,0,0,0q�6,   $	��Top=�6>   $��Center=�6V    J� RenderTransformOrigin$ 0.5,0.5&�6s   $��	0,9.5,0,0q�6�   z�5      V� 5      .~�5      ��  �� ScaleX$
 1.25Q�6-    �� ScaleY$
 1.25Q�6;   �� 5      �� 5      T� 5      5      5      �� 5      $	��Top=�6   $��8,0,8,0q�65   $
��Left=�6F   $��1��6a   .+�5       ��  �� Orientation$ 
Horizontal=�6!   .+�5!      �� -   5         ��Name$ title��5!   $   $
��Wrap=�63   $��Title��6G   $��Normal"�6T   $��14(�6h   $
��Left=�6v   $	��Top=�6�   �� 5"      -   5         ��Name$
 star��5"       $��14��6.   $��14��6:   $
��Left=�6E   $��Center=�6`   $%��/Resources/recommended-star.png��6{   $��8,0,0,0q�6�    .  *launcher_mods_installedTab_recommendedStar	 � ToolTip#	 �   6�   
 � Cursor$

 Help��6�   �� 5#      -   5        $ patch��5#       $��14��6/   $��14��6;   $$��/Resources/patch-gear-icon.png��6F    ( $launcher_mods_installedTab_patchGear#	 �  6n   $

 Help��6�   $��8,0,0,0q�6�   $��Center=�6�   $
��Left=�6�   �� 5$      -   5        $ world��5$       $��14��6/   $��14��6;   $ ��/Resources/world-earth.png��6F    * &launcher_mods_installedTab_customWorld#	 �  6j   $

 Help��6�   $��8,0,0,0q�6�   $��Center=�6�   $
��Left=�6�   �� 5%      -   5        $ merge��5%       $��14��6/   $��14��6;   $��/Resources/merge-icon.png��6F    + 'launcher_mods_installedTab_mergedResult#	 �  6i   $

 Help��6�   $
��Left=�6�   $��Center=�6�   $��8,0,0,0q�6�   5&      �� 5'      -   5        $ category��5'       $
��Wrap=�62   $��Category��6F   $��0,0,0,2q�6V   $��10(�6g   ���	#FFA0A0A06u   �� 5(      -   5        $ modsList��5(       $
��Wrap=�62   $��Place Holder��6F   $��10(�6Z   $y�Visible=�6h   $��0,4,0,0q�6}   5)      �� 5*      $	��Top=�6   $��Right=�65   $��2��6Q   $��0,5,0,0q�6a   $ 
Horizontal=�6r   .+�5+      ;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -	   5          Name$ uninstallButton��5+   0      HoverBackground �	#FFAD2A2A  TheSimsSansSc#���  6e   ���	#FF7D2323���	#FF5D0101
�������$

 Hand��6�   $��24��6�   $��0,0,0,0q�6�   $��Center=�6   $��Center=�6  $��24��68  .��5,      �� $��/Resources/trash-icon.png��6    $��4,4,4,4q�6C   5-      5.      �� 5/      -
   5         ��Name$ 
moreButton��5/      $��4��6-   $��0,0,0,0q�6�   $��Center=�6�   $	��Top=�6�   $��24��6�   $��24��6�   $

 Hand��6�   $��0,5,0,0q�6�   ��6=   Z� ��6S   Z� ��6j   Z� .��50      �� $��Stretch=�6   $��Stretch=�6:   $$��/Resources/three-dots-icon.png��6V   $��2,2,2,2q�6~   51      52      53      55      56      