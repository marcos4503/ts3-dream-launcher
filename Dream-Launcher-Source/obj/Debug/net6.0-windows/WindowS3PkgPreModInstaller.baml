   M S B A M L   `   `   `  ����M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TS3_Dream_LauncherTS3_Dream_Launcher  5   	   3    -TS3_Dream_Launcher.WindowS3PkgPreModInstaller   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:TS3_Dream_Launcher     5   	     � Title$  Place Holder��5   	    � WindowStyle$ 
ToolWindow=�6    � Icon$ /Resources/icon-32x.png��67    � 
ResizeMode$ NoResize=�6V    � ShowInTaskbar$ False��6l    � SizeToContent$ WidthAndHeight=�6�    � Topmost$
 True��6�    � WindowStartupLocation$ CenterScreen=�6�   .��5	      � $	��480��6   $	��400��6   .+�5
   
   � -   5         �Name$ extractionUi��5
      $y�	Collapsed=�6%   .+�5      �� $��Center=�6   $��Center=�66   .+�5      �� -   5        	 ��Name$	 extractTitle��5      $
��Wrap=�62   $��	TextBlock��6F   $	��Top=�6W   $��Center=�6o   � 5      $��12��6   $	��128��6*   $	��Top=�66   $��0,8,0,0q�6N   
 � IsIndeterminate$

 True��6_   5      5      � 5   
   -   5        $ packageList��5      $y�Visible=�6$   .+�5      �� $��0,0,0,42q�6   
�������.��5      �� -   5         ��Name$ pkgList��5      $	��Top=�6.   $��8,8,8,0q�6F   .+�5      �� -   5        $	 pkgListEmpty��5       $
��Wrap=�66   $��	TextBlock��6J   $��Center=�6[   $	��Top=�6x   ���	#FF9E9E9E6�   5      5      �� 5      $	��0.5��6   $��0��6%   
���   �$��0,0,0,42q�6F   $��Bottom=�6X    J� Opacity$	 0.5Q�6s   �� 5      -   5        $	 
pkgListTip��5      $
��Wrap=�6,   $���� dwhjwid qwiydvbwqyi dqwydwqbvyidwqvbd qwdyqw biywqbv dyiqw dyiqwbvdqwybdvqwuyd qwyd qwyid uqyiwd qwuyid vqwuyidwquyid qwuyiqw yiud qwi��6@   $��Bottom=�6�   $��	32,0,32,8q�6�   $��Center=�6�   $��10(�6   �� TextAlignment$ Center=�6(  5      5      5      