   M S B A M L   `   `   `  ����D  @InharoGCS, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null%clr-namespace:InharoGCS	InharoGCS  5          InharoGCS.PlotControl   5      W SPresentationCore, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35U QScottPlot.WPF, Version=4.1.68.0, Culture=neutral, PublicKeyToken=e53b06131e34a3aae aSystem.Windows.Controls.Ribbon, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=6.0.2.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      )localclr-namespace:InharoGCS     5        � 	Resources  5	         MouseOverStyle �� 
TargetType �� Triggers P� Property P� Value �� Property  ColorSelected �� Value  WindowButton �� BasedOn��%�  &	        &	 �     7   �� 5
   
   " ��6'    5      P� 5       �@J�IsMouseOver$
 True��61   . �5      ��  � ��# �  63   5      5      �� 5   
   " ��6%   8   .�5      ��  � ��$ 18��6&   �� 5       � ��$ 0,1,0,1q�6'   �� 5       � ��
 ���� 6+   �� 5       � �� 5      �� 5      .R�5      � #��z��6   .+�5      �� #��z��6/   5      5      5      5      5      .��5       ��  J� Effect 5!   
   3  -System.Windows.Media.Effects.DropShadowEffect  5"      	   Opacity$		 0.2Q�6   
   ShadowDepth$
 3Q�6-   ._�5$   
   �  � RowDefinitions 5%      �� 5&      $��18 �6    �� 5'      $��* �6    .+�5)      �� $��0��6%   $
��Plot��62   $��10(�6A   $��8,3,0,0q�6O     ColorBar#���  6a   
���������6   Z� 6�   �� 5*      #��[ 6   $��Right=�6;   $��0��6W   $��Stretch=�6d   .��5+      �� $��/Images/CloseButton.png��6   $��14��69   $��14��6D   5,        ScottPlot.WpfPlot  5.      -   5          Name$ 
pltControl��5.      $��Stretch=�6*   $��Stretch=�6H   $��1��6d   5/      50      51      