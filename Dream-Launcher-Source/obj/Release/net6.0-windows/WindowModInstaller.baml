   M S B A M L   `   `   `  ���� M  ITS3 Dream Launcher, Version=1.6.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:TS3_Dream_LauncherTS3_Dream_Launcher  c6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton(TS3_Dream_Launcher.Controls.BeautyButton  +    %TS3_Dream_Launcher.WindowModInstaller   W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     @d2http://schemas.microsoft.com/expression/blend/2008     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     2local clr-namespace:TS3_Dream_Launcher     OBeautyButton6clr-namespace:TS3_Dream_Launcher.Controls.BeautyButton       � Title$  Place Holder�� � WindowStyle$ 
ToolWindow=� � Icon$ /Resources/icon-32x.png�� � 
ResizeMode$ NoResize=� � ShowInTaskbar$ False�� � SizeToContent$ WidthAndHeight=� � Topmost$
 True�� � WindowStartupLocation$ CenterScreen=� � 	Resources    
promptExit	 �� 
TargetName
 �� TargetProperty9  3System.Windows.Media.Animation.EasingDoubleKeyFrame O� KeyTime O� Value  installUiEnter��%  &	        &	 �     �� .��U� $	 confirmationPrompt��$
 (UIElement.Opacity)�.\�  $ 00:00:00��$ 1Q�  $ 00:00:00.3000000��$ 0Q��� .��U� $	 	installUi��$
 (UIElement.Opacity)�.\�  $ 00:00:00��$ 0Q�  $ 00:00:00.3000000��$ 1Q�.��� $	��600��$	��512��.+�� -    �Name$ confirmationPrompt��$y�	Collapsed=�.+��� $��Center=�$��Center=�$	��464��.+��� -    ��Name$ question��$
��Wrap=�$��16(�$
��Bold"�  TheSimsSansSc#���  $	��800�� �� TextAlignment$ Center=�$��Place Holder���� -   $ modInfo��$
��Wrap=�$��14(�$��Normal"�$	��800��$ Center=�$��Place Holder��$��0,16,0,0q���  �� Orientation$ 
Horizontal=�$��Center=�$��Center=�$��0,64,0,0q�.+�;   5TS3_Dream_Launcher.Controls.BeautyButton.BeautyButton  -     Name$ no��   HoverBackground �	#FFB34242$	��128��$��34��#���  $��16(����	#FFA00000���	#FF580000
�������$��PH�� � Cursor$
 Hand��  -   $	 yes�� �	#FF00BF0C$	��128��$��34��#���  $��16(����	#FF00A00A���	#FF0F5800
�������$��32,0,0,0q�$��PH��$
 Hand��� -   $ 	installUi��$y�Visible=�.+��� $	��Top=�$��8,8,8,0q�.+��� -   $ title��$
��Wrap=�$��Title��$��24(�$
��Bold"�#���  $��Center=��� $��Center=�$	��Top=�$��0,8,0,0q�$ 
Horizontal=�.+��� -   $
 type��$
��Wrap=�$
��Type��$��0,0,4,0q����	#FF484848�� $��0,0,0,0q�$��4��$��4��
���   � �� CornerRadius$ 4,4,4,4�� J� Opacity$
 0.25Q��� -	   $
 name��$
��Wrap=�$
��Name��$��4,0,0,0q����	#FF484848�� ���	#FFC8C8C8$��1q�$��0,32,0,0q�$ 8,8,8,8��._��� $	��Top=�$��8,8,8,8q�.+��� $ 
Horizontal=�.+�� $��16��$��16��$��0,0,8,0q�.+��� -
    ��Name$ 	step1Busy��$��16��$��16��$%��/Resources/mod-install-busy.png���� -   $ 
step1Error��$��16��$��16��$&�� /Resources/mod-install-error.png���� -   $ step1Success��$��16��$��16��$#��/Resources/mod-install-ok.png���� -   $ step1Pending��$��16��$��16��$(��"/Resources/mod-install-pending.png���� -   $ 
step1Title��$
��Wrap=�$��Title��$
��Left=�$��Normal"��� -    ��Name$ step1Content��$��0,8,0,0q�.+��� -   $ step1ContentText��$
��Wrap=�$��Message��$��Center=�� -    �Name$ step1Progress��$��8��$��0,8,0,0q� � IsIndeterminate$
 True��� -   $ step1SubProgress��$��8��$
 True��$��0,4,0,0q��� ���	#FFC8C8C8$��1q�$ 8,8,8,8��$��0,8,0,0q�._��� $	��Top=�$��8,8,8,8q�.+��� $ 
Horizontal=�.+�� $��16��$��16��$��0,0,8,0q�.+��� -   $ 	step2Busy��$��16��$��16��$%��/Resources/mod-install-busy.png���� -   $ 
step2Error��$��16��$��16��$&�� /Resources/mod-install-error.png��$��0,0,0,0q��� -   $ step2Success��$��16��$��16��$#��/Resources/mod-install-ok.png���� -   $ step2Pending��$��16��$��16��$(��"/Resources/mod-install-pending.png���� -   $ 
step2Title��$
��Wrap=�$��Title��$
��Left=�$��Normal"��� -   $ step2Content��$��0,8,0,0q�.+��� -   $ step2ContentText��$
��Wrap=�$��Message��$��Center=�� -   $ step2Progress��$��8��$��0,8,0,0q�$
 True���� ���	#FFC8C8C8$��1q�$ 8,8,8,8��$��0,8,0,0q�._��� $	��Top=�$��8,8,8,8q�.+��� $ 
Horizontal=�.+�� $��16��$��16��$��0,0,8,0q�.+��� -   $ 	step3Busy��$��16��$��16��$%��/Resources/mod-install-busy.png���� -   $ 
step3Error��$��16��$��16��$&�� /Resources/mod-install-error.png���� -   $ step3Success��$��16��$��16��$#��/Resources/mod-install-ok.png���� -   $ step3Pending��$��16��$��16��$(��"/Resources/mod-install-pending.png���� -   $ 
step3Title��$
��Wrap=�$��Title��$
��Left=�$��Normal"��� -    $ step3Content��$��0,8,0,0q�.+��� -!   $ step3ContentText��$
��Wrap=�$��Message��$��Center=��� $��1,1,1,1q�$��0,5,0,0q����	#FFD6D6D6���	#FF848484._��� -"    ��Name$ conflictsScroll��$	��200��.���� -#   $ conflictsList��$��4,4,4,0q�$	��Top=�� -$   $ step3Progress��$��8��$��0,8,0,0q�$
 True���� ���	#FFC8C8C8$��1q�$ 8,8,8,8��$��0,8,0,0q�._��� $	��Top=�$��8,8,8,8q�.+��� $ 
Horizontal=�.+�� $��16��$��16��$��0,0,8,0q�.+��� -%   $ 	step4Busy��$��16��$��16��$%��/Resources/mod-install-busy.png���� -&   $ 
step4Error��$��16��$��16��$&�� /Resources/mod-install-error.png���� -'   $ step4Success��$��16��$��16��$#��/Resources/mod-install-ok.png���� -(   $ step4Pending��$��16��$��16��$(��"/Resources/mod-install-pending.png���� -)   $ 
step4Title��$
��Wrap=�$��Title��$
��Left=�$��Normal"��� -*   $ step4Content��$��0,8,0,0q�.+��� -+   $ step4ContentText��$
��Wrap=�$��Message��$��Center=�� -,   $ step4Progress��$��8��$��0,8,0,0q�$
 True���� --   $ errorReason��$��Center=�$��Bottom=�$��0,0,0,16q�$ 
Horizontal=�$y�Visible=�.+��� $��16��$��16��$��0,0,8,0q�$%��/Resources/mod-install-fail.png��$��Center=�$��Center=��� -.   $ errorReasonTxt��$
��Wrap=�$��Place Holder��$y�Visible=�
���  ��$
��Left=�$��Center=��� -/   $ 
doneReason��$��Center=�$��Bottom=�$��0,0,0,16q�$ 
Horizontal=�.+��� $��16��$��16��$��0,0,8,0q�$%��/Resources/mod-install-done.png��$��Center=�$��Center=��� -0   $ doneReasonTxt��$
��Wrap=�$��Place Holder��$y�Visible=����	#FF00982C$
��Left=�$��Center=�