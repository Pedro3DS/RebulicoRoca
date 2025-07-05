using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Configurações de Jogo")]
    [SerializeField] private int _initialLives = 3; // Quantidade de vidas para cada jogador no início do jogo
    [SerializeField] private float _delayBeforeSceneReload = 1f; // Atraso antes de recarregar a cena

    [Header("Referência aos Jogadores")]
    [SerializeField] private Player _player1;
    [SerializeField] private Player _player2;

    [Header("UI do Jogo")]
    [SerializeField] private TextMeshProUGUI _winnerTextDisplay;
    [SerializeField] private GameObject _gameOverPanel;

    [Header("UI de Vidas Atuais")]
    [SerializeField] private TextMeshProUGUI _player1LivesDisplay; // Para mostrar as vidas atuais do Player 1
    [SerializeField] private TextMeshProUGUI _player2LivesDisplay; // Para mostrar as vidas atuais do Player 2

    // Chaves para PlayerPrefs
    private const string PLAYER1_LIVES_KEY = "Player1CurrentLives";
    private const string PLAYER2_LIVES_KEY = "Player2CurrentLives";

    private int _player1CurrentLives;
    private int _player2CurrentLives;

    private bool _isGameOver = false; // Flag para controlar o estado final do jogo

    void OnEnable()
    {
        Player.OnPlayerCollided += HandlePlayerCollided;
    }

    void OnDisable()
    {
        Player.OnPlayerCollided -= HandlePlayerCollided;
    }

    void Awake()
    {
        // Se esta é a primeira vez que o jogo inicia, inicializa as vidas no PlayerPrefs
        if (!PlayerPrefs.HasKey(PLAYER1_LIVES_KEY))
        {
            PlayerPrefs.SetInt(PLAYER1_LIVES_KEY, _initialLives);
            PlayerPrefs.SetInt(PLAYER2_LIVES_KEY, _initialLives);
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs de vidas inicializado.");
        }
    }

    void Start()
    {
        InitializeGameRound();
    }

    private void InitializeGameRound()
    {
        _isGameOver = false;
        Time.timeScale = 1; // Garante que o jogo esteja rodando na velocidade normal

        LoadCurrentLives(); // Carrega as vidas do PlayerPrefs
        UpdateLivesUI(); // Atualiza a UI das vidas na cena

        // Reseta os visuais dos jogadores (posição, direção, inicia rastro)
        _player1.ResetPlayerVisuals();
        _player2.ResetPlayerVisuals();

        // Garante que os painéis de UI estejam escondidos
        if (_winnerTextDisplay != null) _winnerTextDisplay.gameObject.SetActive(false);
        if (_gameOverPanel != null) _gameOverPanel.SetActive(false);

        Debug.Log("Rodada iniciada. Vidas P1: " + _player1CurrentLives + ", Vidas P2: " + _player2CurrentLives);
    }

    // Este método é chamado quando um jogador colide
    private void HandlePlayerCollided(string collidedPlayerTag)
    {
        if (_isGameOver) return; // Não faz nada se o jogo já terminou

        Debug.Log($"GameManager: {collidedPlayerTag} colidiu.");

        // Para o jogo imediatamente (para evitar múltiplas colisões/lógica indesejada)
        Time.timeScale = 0; 
        
        // Decrementa a vida do jogador que colidiu
        if (collidedPlayerTag == _player1.PlayerTag)
        {
            _player1CurrentLives--;
        }
        else if (collidedPlayerTag == _player2.PlayerTag)
        {
            _player2CurrentLives--;
        }

        SaveCurrentLives(); // Salva as vidas atualizadas no PlayerPrefs
        UpdateLivesUI(); // Atualiza a UI das vidas

        // Verifica se algum jogador ficou sem vidas
        if (_player1CurrentLives <= 0 || _player2CurrentLives <= 0)
        {
            _isGameOver = true;
            Debug.Log("Fim do Jogo! Um jogador ficou sem vidas.");
            EndGame(); // Finaliza o jogo e mostra o vencedor
        }
        else
        {
            // Se ainda houver vidas, reinicia a cena para uma nova rodada
            Debug.Log("Vidas restantes. Reiniciando cena...");
            StartCoroutine(ReloadSceneRoutine());
        }
    }

    // Rotina para atrasar e recarregar a cena
    IEnumerator ReloadSceneRoutine()
    {
        yield return new WaitForSeconds(_delayBeforeSceneReload);
        // Ao recarregar a cena, todos os objetos são destruídos e recriados.
        // As vidas serão carregadas novamente do PlayerPrefs no Start()
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
    }

    // Finaliza o jogo e determina o vencedor
    private void EndGame()
    {
        string winnerTag = "Nenhum";

        if (_player1CurrentLives <= 0 && _player2CurrentLives <= 0)
        {
            winnerTag = "Empate!";
        }
        else if (_player1CurrentLives <= 0)
        {
            winnerTag = _player2.PlayerTag; // Player 1 perdeu, Player 2 venceu
        }
        else if (_player2CurrentLives <= 0)
        {
            winnerTag = _player1.PlayerTag; // Player 2 perdeu, Player 1 venceu
        }

        DisplayWinner(winnerTag);

        // Opcional: Limpar PlayerPrefs de vidas para um novo jogo completo
        PlayerPrefs.DeleteKey(PLAYER1_LIVES_KEY);
        PlayerPrefs.DeleteKey(PLAYER2_LIVES_KEY);
        PlayerPrefs.Save(); // Salva as alterações
        Debug.Log("Vidas resetadas no PlayerPrefs para um novo jogo.");
    }

    // Exibe o vencedor na tela
    private void DisplayWinner(string winnerTag)
    {
        if (_winnerTextDisplay != null)
        {
            _winnerTextDisplay.text = $"Vencedor: {winnerTag}!";
            _winnerTextDisplay.gameObject.SetActive(true);
        }
        if (_gameOverPanel != null)
        {
            _gameOverPanel.SetActive(true);
        }
    }

    // Carrega as vidas atuais dos jogadores do PlayerPrefs
    private void LoadCurrentLives()
    {
        _player1CurrentLives = PlayerPrefs.GetInt(PLAYER1_LIVES_KEY, _initialLives);
        _player2CurrentLives = PlayerPrefs.GetInt(PLAYER2_LIVES_KEY, _initialLives);
    }

    // Salva as vidas atuais dos jogadores no PlayerPrefs
    private void SaveCurrentLives()
    {
        PlayerPrefs.SetInt(PLAYER1_LIVES_KEY, _player1CurrentLives);
        PlayerPrefs.SetInt(PLAYER2_LIVES_KEY, _player2CurrentLives);
        PlayerPrefs.Save();
    }

    // Atualiza a UI que exibe as vidas atuais
    private void UpdateLivesUI()
    {
        if (_player1LivesDisplay != null)
        {
            _player1LivesDisplay.text = $"P1 Vidas: {_player1CurrentLives}";
        }
        if (_player2LivesDisplay != null)
        {
            _player2LivesDisplay.text = $"P2 Vidas: {_player2CurrentLives}";
        }
    }

    // Método público para reiniciar o jogo completamente (ex: botão de "Novo Jogo")
    public void RestartGameFull()
    {
        // Limpa as vidas no PlayerPrefs para que o jogo recomece do zero
        PlayerPrefs.DeleteKey(PLAYER1_LIVES_KEY);
        PlayerPrefs.DeleteKey(PLAYER2_LIVES_KEY);
        PlayerPrefs.Save();

        Time.timeScale = 1; // Volta a velocidade do tempo ao normal antes de carregar a cena
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Método para resetar o PlayerPrefs se necessário para testes
    public void ResetPlayerPrefsLives()
    {
        PlayerPrefs.DeleteKey(PLAYER1_LIVES_KEY);
        PlayerPrefs.DeleteKey(PLAYER2_LIVES_KEY);
        PlayerPrefs.Save();
        Debug.Log("PlayerPrefs de vidas resetado manualmente.");
        // Opcional: recarregar a cena após resetar PlayerPrefs
        // SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}