using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    // [SerializeField] private GameObject _spawnObject; // Não mais necessário para o visual, agora é para o collider
    [SerializeField] private GameObject _trailColliderPrefab; // Novo: Prefab para o GameObject com o Collider do rastro
    [SerializeField] private Transform _initialSpawnPoint; 
    [SerializeField] private float _speed = 5f; 
    [SerializeField] private string _enemyTag;
    [SerializeField] private string _directionStr;

    [Header("Configurações de Movimento e Rastro")]
    [SerializeField] private float _moveUpdateRate = 0.1f; 
    // private float _nextMoveTime; // Não mais necessário com yield return new WaitForSeconds

    private Vector2 _currentDirection; 
    private Vector2 _inputDirection; 

    private Rigidbody2D _rb;
    private TrailRenderer _trailRenderer; // Referência ao Trail Renderer
    private bool _canMove = true; 

    public delegate void PlayerCollided(string playerTag);
    public static event PlayerCollided OnPlayerCollided; 

    public string PlayerTag => gameObject.tag;

    void Awake()
    {
        // Garante que o Trail Renderer exista no objeto
        _trailRenderer = GetComponent<TrailRenderer>();
        if (_trailRenderer == null)
        {
            Debug.LogError("TrailRenderer não encontrado no GameObject do Player! Adicione um componente TrailRenderer.");
        }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        
        // Define a direção inicial e rotação
        if (gameObject.CompareTag("Player1")) // Lógica para o Player 1 virar para baixo
        {
            _currentDirection = Vector2.down;
            transform.rotation = Quaternion.Euler(0, 0, 0); 
        }
        else // Para o Player 2
        {
            if (_directionStr == "up")
            {
                _currentDirection = Vector2.up;
                transform.rotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                _currentDirection = Vector2.down;
                transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
        _inputDirection = _currentDirection; // Inicializa com a mesma direção

        // Inicia o rastro visual do Trail Renderer
        if (_trailRenderer != null)
        {
            _trailRenderer.Clear(); // Limpa qualquer rastro residual no início
            _trailRenderer.emitting = true; // Começa a emitir o rastro
        }
        
        StartCoroutine(MoveAndSpawnRoutine());
    }

    void Update()
    {
        HandleInput();
    }
    
    void FixedUpdate() 
    {
        _rb.velocity = Vector2.zero; 
    }

    IEnumerator MoveAndSpawnRoutine()
    {
        while (_canMove && Time.timeScale > 0) 
        {
            yield return new WaitForSeconds(_moveUpdateRate); 

            if (Time.timeScale == 0)
            {
                _rb.velocity = Vector2.zero;
                yield break; 
            }

            if (_inputDirection != -_currentDirection)
            {
                _currentDirection = _inputDirection;
            }

            // **AGORA: Insta o OBJETO COLLIDER DO RASTRO**
            // Cria o objeto de colisão na posição atual antes de mover
            GameObject trailCollider = Instantiate(_trailColliderPrefab, transform.position, Quaternion.identity);
            trailCollider.tag = $"body{gameObject.tag}"; // Define a tag para colisão
            // Opcional: Garanta que ele não seja destruído imediatamente (pode ajustar o lifetime)
            // Ou o GameManager pode destruir todos eles na troca de rodada/cena
            // Ex: Destroy(trailCollider, _trailRenderer.time + 0.1f); // Destrói após o tempo de vida do rastro + um pouco

            // Move o cavalo para a nova posição (teletransporte)
            float newX = Mathf.RoundToInt(transform.position.x + _currentDirection.x * _speed * _moveUpdateRate);
            float newY = Mathf.RoundToInt(transform.position.y + _currentDirection.y * _speed * _moveUpdateRate);
            transform.position = new Vector2(newX, newY);

            // Ajusta a rotação do cavalo para a nova direção
            UpdateRotation();
        }
        _rb.velocity = Vector2.zero; 
    }

    private void UpdateRotation()
    {
        if (_currentDirection == Vector2.up)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); 
        }
        else if (_currentDirection == Vector2.down)
        {
            transform.rotation = Quaternion.Euler(0, 0, 180); 
        }
        else if (_currentDirection == Vector2.left)
        {
            transform.rotation = Quaternion.Euler(0, 0, 90); 
        }
        else if (_currentDirection == Vector2.right)
        {
            transform.rotation = Quaternion.Euler(0, 0, 270); 
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.timeScale > 0 && _canMove &&
            (collision.gameObject.CompareTag(_enemyTag) || 
             collision.gameObject.CompareTag($"body{_enemyTag}") || // Colisão com rastro do inimigo
             collision.gameObject.CompareTag($"body{gameObject.tag}"))) // Colisão com o próprio rastro
        {
            Debug.Log($"{gameObject.tag} colidiu com {collision.gameObject.tag}.");
            _canMove = false; 
            OnPlayerCollided?.Invoke(gameObject.tag); 
            _rb.velocity = Vector2.zero; 
            StopAllCoroutines(); 

            // Para de emitir o rastro visual imediatamente após a colisão
            if (_trailRenderer != null)
            {
                _trailRenderer.emitting = false;
            }
        }
    }

    public void ResetPlayerVisuals()
    {
        // **Limpa o rastro visual do Trail Renderer**
        if (_trailRenderer != null)
        {
            _trailRenderer.emitting = false; // Para de emitir antes de limpar
            _trailRenderer.Clear(); // Limpa todos os pontos do rastro
            _trailRenderer.emitting = true; // Volta a emitir para a nova rodada
        }

        // Destrói todos os objetos COLIDERS de rastro deste jogador
        // Isso é importante, pois o GameManager só recarrega a cena no final
        // mas o Player precisa limpar seus próprios colliders em cada reset visual.
        // O GameManager vai fazer o trabalho pesado de destruir todos os colliders
        // na recarga da cena ou no fim do jogo.
        GameObject[] trailColliders = GameObject.FindGameObjectsWithTag($"body{gameObject.tag}");
        foreach (GameObject obj in trailColliders)
        {
            Destroy(obj);
        }

        transform.position = _initialSpawnPoint.position;
        
        // Lógica de direção e rotação inicial (igual ao Start)
        if (gameObject.CompareTag("Player1")) 
        {
            _currentDirection = Vector2.down; 
            transform.rotation = Quaternion.Euler(0, 0, 0); 
        }
        else 
        {
            if (_directionStr == "up")
            {
                _currentDirection = Vector2.up;
                transform.rotation = Quaternion.Euler(0, 0, 0); 
            }
            else
            {
                _currentDirection = Vector2.down;
                transform.rotation = Quaternion.Euler(0, 0, 180); 
            }
        }
        _inputDirection = _currentDirection; 
        
        gameObject.SetActive(true);
        _canMove = true; 
        StopAllCoroutines(); 
        StartCoroutine(MoveAndSpawnRoutine()); 
    }

    private void HandleInput()
    {
        if (Time.timeScale == 0 || !_canMove) return;

        Vector2 newAttemptedDirection = _inputDirection; 

        // ... (lógica de input permanece a mesma) ...
        if (_directionStr != "up") // Player 1 (WASD)
        {
            if (Input.GetKeyDown(KeyCode.W)) newAttemptedDirection = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.S)) newAttemptedDirection = Vector2.down;
            else if (Input.GetKeyDown(KeyCode.A)) newAttemptedDirection = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.D)) newAttemptedDirection = Vector2.right;
        }
        else // Player 2 (Setas)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) newAttemptedDirection = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow)) newAttemptedDirection = Vector2.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) newAttemptedDirection = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow)) newAttemptedDirection = Vector2.right;
        }

        if (newAttemptedDirection != -_currentDirection) 
        {
            _inputDirection = newAttemptedDirection;
        }
    }
}