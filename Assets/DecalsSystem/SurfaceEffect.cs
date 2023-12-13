using GameGC.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

[CreateAssetMenu(menuName = "Surface/Effect", fileName = "SurfaceEffect", order = 0)]
public class SurfaceEffect : ScriptableObject
{
    [Header("Decals & Effects")]
    public GameObject[] decalsVariant;
    public Vector2 minMaxRandomScale = Vector2.one;
    public float spawnDistance = 0.01f;
    public SNullable<float> destroyTimer =5;

    [Header("Audio")]
    public Object Audio;
}

    
    public struct EasyGUI
    {
        private const float SingleLineHeight = 18f;
        
        private Rect _rect;
        private Vector2 _position;
        public EasyGUI(Rect rect)
        {
            _rect = rect;
            _position = new Vector2(rect.x, rect.y);
        }
        public void NextLine(float height, out Rect rect)
        {
            _position.x = _rect.x;
            rect = new Rect(_position, new Vector2(_rect.width, height));
            _position += Vector2.up * height;
        }
        
        public void NextSingleLine(out Rect rect)
        {
            _position.x = _rect.x;
            rect = new Rect(_position, new Vector2(_rect.width, SingleLineHeight));
            _position += Vector2.up * SingleLineHeight;
        }
        
        public void NextHalfLine(float height,float widthMultiplier, out Rect rect)
        {
            var size = new Vector2(_rect.width * widthMultiplier, height);
            rect = new Rect(_position, size);
            _position += size;
        }
        
        public void NextHalfSingleLine(float widthMultiplier, out Rect rect)
        {
            var size = new Vector2(_rect.width * widthMultiplier, SingleLineHeight);
            rect = new Rect(_position, size);
            _position += size;
        }
        
        public void CurrentHalfLine(float height,float widthMultiplier, out Rect rect)
        {
            float width = _rect.width * widthMultiplier;
            rect = new Rect(_position,  new Vector2(width, height));
            _position += Vector2.right * width;
        }
        
        public void CurrentHalfSingleLine(float widthMultiplier, out Rect rect)
        {
            float width = _rect.width * widthMultiplier;
            rect = new Rect(_position,  new Vector2(width, SingleLineHeight));
            _position += Vector2.right * width;
        }
        
        public void CurrentAmountLine(float height,float width, out Rect rect)
        {
            rect = new Rect(_position,  new Vector2(width, height));
            _position += Vector2.right * width;
        }
        
        public void CurrentAmountSingleLine(float width,out Rect rect)
        {
            rect = new Rect(_position,  new Vector2(width, SingleLineHeight));
            _position += Vector2.right * width;
        }
    }

