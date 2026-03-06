using UnityEngine;
using TMPro;

public class HUD : MonoBehaviour
{
    [Header("Referências")]
    public Controller playerController;
    public TextMeshProUGUI chargesText;

    // Caracteres visuais (Você pode copiar e colar outros simbolos aqui se quiser)
    private string filledSquare = "■"; // Quadrado Cheio
    private string emptySquare = "■";  // Usaremos o mesmo quadrado, mas mudaremos a cor no código

    void Update()
    {
        if (playerController != null && chargesText != null)
        {
            int current = playerController.GetCharges();
            int max = playerController.maxCharges; // Acessando a variável pública do Controller

            // Começamos o texto vazio
            string visualBar = "";

            // Loop para desenhar os quadrados
            for (int i = 0; i < max; i++)
            {
                if (i < current)
                {
                    // Se o índice for menor que a carga atual, desenha BRANCO (Ativo)
                    visualBar += $"<color=white>{filledSquare}</color> ";
                }
                else
                {
                    // Se não, desenha CINZA ESCURO (Gasto/Vazio)
                    // Usamos Rich Text do TextMeshPro para mudar a cor no meio da frase
                    visualBar += $"<color=#444444>{emptySquare}</color> ";
                }
            }

            chargesText.text = visualBar;
        }
    }
}
