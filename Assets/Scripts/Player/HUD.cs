using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("Referências")]
    public Controller playerController;
    public TextMeshProUGUI chargesText;

    private string filledSquare = "■";
    private string emptySquare = "■"; 

    void Update()
    {
        if (playerController != null && chargesText != null)
        {
            int current = playerController.GetCharges();
            int max = playerController.maxCharges; 
            string visualBar = "";


            for (int i = 0; i < max; i++)
            {
                if (i < current)
                {
                    visualBar += $"<color=white>{filledSquare}</color> ";
                }
                else
                {
                    visualBar += $"<color=#444444>{emptySquare}</color> ";
                }
            }

            chargesText.text = visualBar;
        }
    }
}
