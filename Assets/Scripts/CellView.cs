using UnityEngine;
using UnityEngine.UI;

public class CellView : MonoBehaviour
{
    private Cell _cell;
    public PieceView PieceView;

    private Image _image;

    private Color _curentColor;
    private Color _tempColor = Color.lightGreen;


    private void Awake()
    {
        _image = GetComponent<Image>();
        _curentColor = _image.color;
    }

    public void ChangeColor()
    {
        _image.color = _tempColor;
    }

    public void ResetColor()
    {
        _image.color = _curentColor;
    }

    public void SetCell(Cell cell)
    {
        _cell = cell;
    }

    public void SetPieceView(PieceView view)
    {
        PieceView = view;
    }
}