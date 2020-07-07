using UnityEngine;

public class TrapController: MonoBehaviour
{

  [SerializeField] public PlayerColor Color;
  
  private float _contdown;
  private float _colorIntensity;
  private bool _disappearAnimation;

  private Material _material;

  void Start()
  {
    SetColor(this.Color);
  }

  public void SetColor(PlayerColor color)
  {
    this.Color = color;

    _material = GetComponent<Renderer>().material;
    _material.SetColor("_Color", PlayerColorMethods.ColorToRGB(this.Color));

    ShowColor();
  }

  public void ShowColor()
  {
    _contdown = 3f;
    _disappearAnimation = false;
    _colorIntensity = 1f;
  }

  void Update()
  {
    _material.SetFloat("_ColorIntensity", _colorIntensity);
    if (_disappearAnimation)
    {
      if (_colorIntensity > 0)
      {
        _colorIntensity -= Time.deltaTime;
      }
      else
      {
        _colorIntensity = 0;
        _disappearAnimation = false;
      }
    }
    else if (_contdown > 0)
    {
      _contdown -= Time.deltaTime;
      if (_contdown <= 0)
      {
        _disappearAnimation = true;
      }
    }
  }


  void OnTriggerStay2D(Collider2D other) {
    if (other.gameObject.tag == "Player") 
    {
      PlayerController player = other.gameObject.GetComponent<PlayerController>();
      if (player.Color == this.Color){
        player.Die();
      }
    }
  }
}
