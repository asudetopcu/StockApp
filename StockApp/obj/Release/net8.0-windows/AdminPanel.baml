   M S B A M L   `   `   `  ����   StockApp    StockApp.AdminPanel   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml       � Title$  Admin Panel��$	��709��$
��1231��.���  � RowDefinitions �� $
��442* ���  � ColumnDefinitions �� $
��628* ��� $
��479* �.+��� -   $��2��.@��� $��Customer Management��.��� $��10q�.+�&   System.Windows.Controls.DataGrid  -     Name$ dataGridCustomers��   AutoGenerateColumns$
 True��$��Stretch=�$	��Top=�$	��400����  �� Orientation$ 
Horizontal=�$
��Left=�$��Bottom=�$��	10,10,0,0q�.+��� -   $��Load Customers��$	��150���� -   $��Generate Customers��$	��150��$��10,0,0,0q��� -   $��log Out��$	��100��$��10,0,0,0q����#B22222
��������� $��Product Management��.��� $��10q�.+��� $ Vertical=�.+��� $��Product List��$
��Bold"�$��16(�$��5q�  -   $ dataGridProducts��$
 True��$	��Top=�$	��300��$��5q��� 
�������$��1q�$��5q�._��� $ Vertical=�$��10q�.+��� $��Stock Status Chart��$
��Bold"�$��14(�$��5q��� -    ��Name$ stockBarChart��$��Center=�$��Center=��� $ 
Horizontal=�$��5q�$��Bottom=�.+�� $��5q�.+��� -    ��Name$ txtProductId��$	��100�� � ToolTip$ Enter Product ID���� $��
Product ID��
�������	 J� IsHitTestVisible$	 False��$��Center=�$��5,2,0,0q�$��Italic$�y��� *Text.Length+
 �� ElementName$
 txtProductId��   LengthToVisibilityConverter �� 	Converter# [  � $��5q�.+��� -	   $ txtProductName��$	��150��$ Enter Product Name���� $��Product Name��
�������$	 False��$��Center=�$��5,2,0,0q�$��Italic$�y��� *Text.Length+$
 txtProductName��# [  � $��5q�.+��� -
   $ txtStock��$	��100��$ Enter Stock Quantity���� $��Stock Quantity��
�������$	 False��$��Center=�$��5,2,0,0q�$��Italic$�y��� *Text.Length+$
 txtStock��# [  � $��5q�.+��� -   $ txtPrice��$	��100��$ Enter Price���� $��Price��
�������$	 False��$��Center=�$��5,2,0,0q�$��Italic$�y��� *Text.Length+$
 txtPrice��# [  �� $ 
Horizontal=�$��5q�.+��� -   $	��Add��$	��100��$��5q��� -   $��Update��$	��100��$��5q��� -   $��Delete��$	��100��$��5q��� -   $��Refresh��$	��100��$��5q��� $
��Logs��.��� $��10q�.+�  -   $ dataGridLogs��$
 True��$��Stretch=�$��Stretch=��� -   $��Refresh Logs��$��Right=�$��Bottom=�$��10q��� $��Order Management��.��� $��10q�.+�  -   $ dataGridQueue��$
 True��$��Stretch=�$��Stretch=��� -   $��Approve Next Order��$��
10,630,0,0q�$	��150���� -   $��Refresh Queue��$��
10,630,0,0q�$	��150���� $ 
Horizontal=�$��Bottom=�$��452,0,455,68q�.+�� $��5q�.+��� -   $ 
txtOrderId��$	��150��$ Enter Order ID���� $��Order ID��
�������$��10,2,0,0q�$	 False��$��Center=�$��Italic$�y��� *Text.Length+$
 
txtOrderId��# [  �� $ 
Horizontal=�$��5q�.+��� -   $��Approve��$	��120��$	��100q��� -   $��Cancel Order��$	��120��$��5q�� -    �Name$ progressBar��$��20��$��
10,0,10,10q�$��Bottom=�$y�	Collapsed=� � IsIndeterminate$
 True��$��2��