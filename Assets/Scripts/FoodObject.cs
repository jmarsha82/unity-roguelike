using UnityEngine;

public class FoodObject : CellObject
{
   public int AmountGranted = 10;

   public override void PlayerEntered()
   {
       GameManager.Instance.BoardManager.ClearCellObject(m_Cell, this);
       Destroy(gameObject);
      
       //increase food
       GameManager.Instance.ChangeFood(AmountGranted);
   }
}
