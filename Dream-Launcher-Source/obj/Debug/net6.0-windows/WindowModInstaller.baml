   M S B A M L   `   `   `  ����M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TS3_Dream_LauncherTS3_Dream_Launcher  5   	   c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  5   	   +    %TS3_Dream_Launcher.WindowModInstaller   5   	   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:TS3_Dream_Launcher     5   	   OBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton     5   	     � Title$  Place Holder��5	   	    � WindowStyle$ 
ToolWindow=�6    � Icon$ /Resources/icon-32x.png��67    � 
ResizeMode$ NoResize=�6V    � ShowInTaskbar$ False��6l    � SizeToContent$ WidthAndHeight=�6�    � Topmost$
 True��6�    � WindowStartupLocation$ CenterScreen=�6�    � 	Resources 5
         
promptExit	 �� 
TargetName
 �� TargetProperty9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value  installUiEnter��%�  &	        &	 �     �� 5   
   .��5      U� $	 confirmationPrompt��6,   $
 (UIElement.Opacity)�6W   .\�5        $ 00:00:00��6'   $ 1Q�6:     5      $ 00:00:00.3000000��6'   $ 0Q�6B   5      5      �� 5   
   .��5      U� $	 	installUi��6,   $
 (UIElement.Opacity)�6N   .\�5        $ 00:00:00��6'   $ 0Q�6:     5      $ 00:00:00.3000000��6'   $ 1Q�6B   5      5      5      .��5      � $	��600��6   $	��512��6   .+�5   
   � -   5         �Name$ confirmationPrompt��5      $y�	Collapsed=�6+   .+�5      �� $��Center=�6   $��Center=�66   $	��464��6Q   .+�5      �� -   5         ��Name$ question��5      $
��Wrap=�6.   $��16(�6B   $
��Bold"�6P     TheSimsSansSc#���  6b   $	��800��6�    �� TextAlignment$ Center=�6�   $��Place Holder��6�   �� 5      -   5        $ modInfo��5      $
��Wrap=�6-   $��14(�6A   $��Normal"�6O   $	��800��6c   $ Center=�6r   $��Place Holder��6�   $��0,16,0,0q�6�   �� 5       �� Orientation$ 
Horizontal=�6   $��Center=�66   $��Center=�6S   $��0,64,0,0q�6n   .+�5      ;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -   5          Name$ no��5   0      HoverBackground �	#FFB34242$	��128��6X   $��34��6d   #���  6p   $��16(�6�   ���	#FFA00000���	#FF580000
�������$��PH��6�    � Cursor$
 Hand��6�     5      -   5        $	 yes��5   0    �	#FF00BF0C$	��128��6Y   $��34��6e   #���  6q   $��16(�6�   ���	#FF00A00A���	#FF0F5800
�������$��32,0,0,0q�6�   $��PH��6   $
 Hand��6  5       5!      5"      � 5#   
   -   5        $ 	installUi��5#      $y�Visible=�6"   .+�5$      �� $	��Top=�6   $��8,8,8,0q�61   .+�5%      �� -   5        $ title��5%      $
��Wrap=�6+   $��Title��6?   $��24(�6L   $
��Bold"�6Z   #���  6l   $��Center=�6�   �� 5&      $��Center=�6   $	��Top=�6:   $��0,8,0,0q�6R   $ 
Horizontal=�6c   .+�5'      �� -   5        $
 type��5'       $
��Wrap=�6.   $
��Type��6B   $��0,0,4,0q�6N   ���	#FF4848486_   �� 5(      $��0,0,0,0q�6   $��4��67   $��4��6A   
���   � �� CornerRadius$ 4,4,4,4��6_    J� Opacity$
 0.25Q�6v   �� 5)      -	   5        $
 name��5)       $
��Wrap=�6.   $
��Name��6B   $��4,0,0,0q�6N   ���	#FF4848486_   5*      �� 5+      ���	#FFC8C8C8$��1q�61   $��0,32,0,0q�6E   $ 8,8,8,8��6W   ._�5,      �� $	��Top=�6!   $��8,8,8,8q�69   .+�5-      �� $ 
Horizontal=�6%   .+�5.      � $��16��6#   $��16��6/   $��0,0,8,0q�6:   .+�5/   "   �� -
   5         ��Name$ 	step1Busy��5/   (   $��16��6;   $��16��6G   $%��/Resources/mod-install-busy.png��6R   �� 50   "   -   5        $ 
step1Error��50   (   $��16��6<   $��16��6H   $&�� /Resources/mod-install-error.png��6S   �� 51   "   -   5        $ step1Success��51   (   $��16��6>   $��16��6J   $#��/Resources/mod-install-ok.png��6U   �� 52   "   -   5        $ step1Pending��52   (   $��16��6>   $��16��6J   $(��"/Resources/mod-install-pending.png��6U   53      �� 54      -   5        $ 
step1Title��54   (   $
��Wrap=�6<   $��Title��6P   $
��Left=�6]   $��Normal"�6x   55      �� 56      -   5         ��Name$ step1Content��56   %   $��0,8,0,0q�6;   .+�57      �� -   5        $ step1ContentText��57   (   $
��Wrap=�6B   $��Message��6V   $��Center=�6e   � 58      -   5         �Name$ step1Progress��58   *   $��8��6A   $��0,8,0,0q�6L    � IsIndeterminate$
 True��6]   � 59      -   5        $ step1SubProgress��59   *   $��8��6D   $
 True��6O   $��0,4,0,0q�6f   5:      5;      5<      �� 5=      ���	#FFC8C8C8$��1q�61   $ 8,8,8,8��6E   $��0,8,0,0q�6\   ._�5>      �� $	��Top=�6!   $��8,8,8,8q�69   .+�5?      �� $ 
Horizontal=�6%   .+�5@      � $��16��6#   $��16��6/   $��0,0,8,0q�6:   .+�5A   "   �� -   5        $ 	step2Busy��5A   (   $��16��6;   $��16��6G   $%��/Resources/mod-install-busy.png��6R   �� 5B   "   -   5        $ 
step2Error��5B   (   $��16��6<   $��16��6H   $&�� /Resources/mod-install-error.png��6S   $��0,0,0,0q�6}   �� 5C   "   -   5        $ step2Success��5C   (   $��16��6>   $��16��6J   $#��/Resources/mod-install-ok.png��6U   �� 5D   "   -   5        $ step2Pending��5D   (   $��16��6>   $��16��6J   $(��"/Resources/mod-install-pending.png��6U   5E      �� 5F      -   5        $ 
step2Title��5F   (   $
��Wrap=�6<   $��Title��6P   $
��Left=�6]   $��Normal"�6x   5G      �� 5H      -   5        $ step2Content��5H   %   $��0,8,0,0q�6;   .+�5I      �� -   5        $ step2ContentText��5I   (   $
��Wrap=�6B   $��Message��6V   $��Center=�6e   � 5J      -   5        $ step2Progress��5J   *   $��8��6A   $��0,8,0,0q�6L   $
 True��6]   5K      5L      5M      �� 5N      ���	#FFC8C8C8$��1q�61   $ 8,8,8,8��6E   $��0,8,0,0q�6\   ._�5O      �� $	��Top=�6!   $��8,8,8,8q�69   .+�5P      �� $ 
Horizontal=�6%   .+�5Q      � $��16��6#   $��16��6/   $��0,0,8,0q�6:   .+�5R   "   �� -   5        $ 	step3Busy��5R   (   $��16��6;   $��16��6G   $%��/Resources/mod-install-busy.png��6R   �� 5S   "   -   5        $ 
step3Error��5S   (   $��16��6<   $��16��6H   $&�� /Resources/mod-install-error.png��6S   �� 5T   "   -   5        $ step3Success��5T   (   $��16��6>   $��16��6J   $#��/Resources/mod-install-ok.png��6U   �� 5U   "   -   5        $ step3Pending��5U   (   $��16��6>   $��16��6J   $(��"/Resources/mod-install-pending.png��6U   5V      �� 5W      -   5        $ 
step3Title��5W   (   $
��Wrap=�6<   $��Title��6P   $
��Left=�6]   $��Normal"�6x   5X      �� 5Y      -    5        $ step3Content��5Y   %   $��0,8,0,0q�6;   .+�5Z      �� -!   5        $ step3ContentText��5Z   (   $
��Wrap=�6B   $��Message��6V   $��Center=�6e   �� 5[      $��1,1,1,1q�6%   $��0,5,0,0q�6?   ���	#FFD6D6D6���	#FF848484._�5\   "   �� -"   5         ��Name$ conflictsScroll��5\   /   $	��200��6H   .��5]   &   �� -#   5        $ conflictsList��5]   1   $��4,4,4,0q�6H   $	��Top=�6Y   5^   #   5_      � 5`      -$   5        $ step3Progress��5`   *   $��8��6A   $��0,8,0,0q�6L   $
 True��6]   5a      5b      5c      �� 5d      ���	#FFC8C8C8$��1q�61   $ 8,8,8,8��6E   $��0,8,0,0q�6\   ._�5e      �� $	��Top=�6!   $��8,8,8,8q�69   .+�5f      �� $ 
Horizontal=�6%   .+�5g      � $��16��6#   $��16��6/   $��0,0,8,0q�6:   .+�5h   "   �� -%   5        $ 	step4Busy��5h   (   $��16��6;   $��16��6G   $%��/Resources/mod-install-busy.png��6R   �� 5i   "   -&   5        $ 
step4Error��5i   (   $��16��6<   $��16��6H   $&�� /Resources/mod-install-error.png��6S   �� 5j   "   -'   5        $ step4Success��5j   (   $��16��6>   $��16��6J   $#��/Resources/mod-install-ok.png��6U   �� 5k   "   -(   5        $ step4Pending��5k   (   $��16��6>   $��16��6J   $(��"/Resources/mod-install-pending.png��6U   5l      �� 5m      -)   5        $ 
step4Title��5m   (   $
��Wrap=�6<   $��Title��6P   $
��Left=�6]   $��Normal"�6x   5n      �� 5o      -*   5        $ step4Content��5o   %   $��0,8,0,0q�6;   .+�5p      �� -+   5        $ step4ContentText��5p   (   $
��Wrap=�6B   $��Message��6V   $��Center=�6e   � 5q      -,   5        $ step4Progress��5q   *   $��8��6A   $��0,8,0,0q�6L   $
 True��6]   5r      5s      5t      5u      �� 5v      --   5        $ errorReason��5v      $��Center=�6.   $��Bottom=�6K   $��0,0,0,16q�6f   $ 
Horizontal=�6x   $y�Visible=�6�   .+�5w      �� $��16��6   $��16��6$   $��0,0,8,0q�6/   $%��/Resources/mod-install-fail.png��6@   $��Center=�6i   $��Center=�6�   �� 5x      -.   5        $ errorReasonTxt��5x      $
��Wrap=�64   $��Place Holder��6H   $y�Visible=�6\   
���  ��$
��Left=�6�   $��Center=�6�   5y      �� 5z      -/   5        $ 
doneReason��5z      $��Center=�6-   $��Bottom=�6J   $��0,0,0,16q�6e   $ 
Horizontal=�6w   .+�5{      �� $��16��6   $��16��6$   $��0,0,8,0q�6/   $%��/Resources/mod-install-done.png��6@   $��Center=�6i   $��Center=�6�   �� 5|      -0   5        $ doneReasonTxt��5|      $
��Wrap=�63   $��Place Holder��6G   $y�Visible=�6[   ���	#FF00982C$
��Left=�6�   $��Center=�6�   5}      5~      5�      5�      