   M S B A M L   `   `   `  ���� M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null]3clr-namespace:TS3_Dream_Launcher.Controls.ListItems%TS3_Dream_Launcher.Controls.ListItems  c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  8    2TS3_Dream_Launcher.Controls.ListItems.StoreModItem   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     Elocal3clr-namespace:TS3_Dream_Launcher.Controls.ListItems     OBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton     .��� .+��� -     ��Name$  
background�����	#FF969696$��1,1,1,1q�$	��Top=� �� CornerRadius$ 4,4,4,4��._�� $��4,4,4,4q� � ColumnDefinitions �� $��24 ��� $��* ��� $	��100 ��� $��8 ��� $��24 �.+��� -   $  status��
���   �$��0,0,0,0q�$��16��$��16��$��Center=�$	��Top=�$ 16,16,16,16��
���   �$��0,8,0,0q��� $	��Top=�$��8,0,8,0q�$
��Left=�$��1��.+��� -    ��Name$ title��$
��Wrap=�$��Title��$��Normal"�$��14(�$
��Left=�$	��Top=��� $	��Top=�$
��Left=� �� Orientation$ 
Horizontal=�.+��� -   $ category��$
��Wrap=�$��Category��$��10(����	#FFA0A0A0�� $��0,0,0,0q�$��4��$��4��
���   �$ 4,4,4,4��$��Center=�$
��Left=�$��4,0,4,0q� J� Opacity$
 0.25Q��� -   $ author��$
��Wrap=�$��	by Author��$��10(����	#FFA0A0A0�� -   $ description��$
��Wrap=�$��Description�����	#FF525252$��0,4,0,8q�$
��Left=�$	��Top=��� $
��Left=�$	��Top=�$ 
Horizontal=�.+��� -    ��Name$	 ep1��$��14��$��/Resources/dlc_ep1s.png��$��14��$��Center=�$
��Left=�$��0,0,4,0q�   launcher_dlc_ep1 � ToolTip# �    � Cursor$
 Help���� -   $	 ep2��$��14��$��/Resources/dlc_ep2s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep2# �  $
 Help���� -	   $	 ep3��$��14��$��/Resources/dlc_ep3s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep3# �  $
 Help���� -
   $	 ep4��$��14��$��/Resources/dlc_ep4s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep4# �  $
 Help���� -   $	 ep5��$��14��$��/Resources/dlc_ep5s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep5# �  $
 Help���� -   $	 ep6��$��14��$��/Resources/dlc_ep6s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep6# �  $
 Help���� -   $	 ep7��$��14��$��/Resources/dlc_ep7s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep7# �  $
 Help���� -   $	 ep8��$��14��$��/Resources/dlc_ep8s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep8# �  $
 Help���� -   $	 ep9��$��14��$��/Resources/dlc_ep9s.png��$��14��$��0,0,4,0q�  launcher_dlc_ep9# �  $
 Help���� -   $
 ep10��$��14��$��/Resources/dlc_ep10s.png��$��14��$��0,0,4,0q� 	 launcher_dlc_ep10# � 	 $
 Help���� -   $
 ep11��$��14��$��/Resources/dlc_ep11s.png��$��14��$��0,0,4,0q� 
 launcher_dlc_ep11# � 
 $
 Help���� $	��Top=�$��Right=�$��2��$��0,8,0,0q�$ 
Horizontal=�.+�;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -   	  Name$	 guideButton��
   HoverBackground
 �	#FF006FD6  TheSimsSansSc#���  ���	#FF005EB5���	#FF002344
�������$
 Hand��$��24��$��0,0,0,0q�$��24�� % !launcher_mods_addNewTab_guideIcon# �  .���� $��/Resources/mod-guide.png��$��4,4,4,4q�  -   $	 problemButton��
 �	#FFAD3434#���  ���	#FF882828���	#FF441212
�������$
 Help��$��24��$��0,0,0,0q�$��24�� ' #launcher_mods_addNewTab_problemIcon# �  $��8,0,0,0q�.���� $ ��/Resources/mod-problem.png��$��4,4,4,4q�  -   $	 	addButton��
 �	#FF3AA42B#���  ���	#FF348828���	#FF071F04
�������$
 Hand��$��24��$��0,0,0,0q�$��Center=�$��Center=�$��24��$��8,0,0,0q�.���� $��/Resources/add-mod.png��$��4,4,4,4q��� -    ��Name$ 
moreButton��$��4��$��0,0,0,0q�$��Center=�$	��Top=�$��24��$��24��$
 Hand��$��0,8,0,0q���Z� ��Z� ��Z� .���� $��Stretch=�$��Stretch=�$$��/Resources/three-dots-icon.png��$��2,2,2,2q�