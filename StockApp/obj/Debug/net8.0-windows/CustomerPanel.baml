   M S B A M L   `   `   `  ����  StockApp    StockApp.CustomerPanel   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Customer Panel��5   	   $	��600��6    $	��800��6-   .��5      �� $��10q�6    �� Orientation$ Vertical=�6   .+�5   
   �� $ 
Horizontal=�6   $��0,0,0,10q�6.   .+�5	      �� $��	Welcome, ��6   $
��Bold"�6)   �� 5
      -   5         ��Name$ textBlockCustomerName��5
      $
��Bold"�67   �� 5      $�� | Current Budget: ��6   $	��5,0q�63   �� 5      -   5        $ textBlockCustomerBudget��5      $
��Bold"�69   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ dataGridProducts��5      $��0,0,0,10q�6-      AutoGenerateColumns$
 True��5         
IsReadOnly$
 True��6.   $	��300��6@   �� 5   
   $ Vertical=�6   $��Center=�6,   .+�5      �� $��Select Product��6   $��5q�6.   �� 5      -   5         ��Name$ comboBoxProducts��5      $	��200��61   $��5q�6=   �� 5      $��Quantity��6   $��5q�6(   �� 5      -   5         ��Name$ textBoxQuantity��5      $	��200��6/   $��5q�6;   �� 5      -   5        $��Place Order��5      $	��100��6+   $��5q�67   6B   �� 5      -   5        $��Log Out��5      $	��100��6   $��10q�6*   ���#B22222
�������6^   5      5      5      