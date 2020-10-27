using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ImageProcessing {
        
    public static class TextureExtentions
        {
            public static Texture2D ToTexture2D(this Texture texture)
            {
                return Texture2D.CreateExternalTexture(
                    texture.width,
                    texture.height,
                    TextureFormat.RGB24,
                    false, false,
                    texture.GetNativeTexturePtr());
            }
        }

    public class Background {
        
        public const string IMAGE_URL = "https://source.unsplash.com/1600x900/?interiors,home,inside";
        public Sprite sprite;
        public Image image;

        public Background(Image image)
        {
            this.image = image;
        }

        public IEnumerator GetTexture() {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(IMAGE_URL);
            yield return www.SendWebRequest();

            if(www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            }
            else {
                Texture2D texture = ((DownloadHandlerTexture)www.downloadHandler).texture.ToTexture2D();
                sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
                image.sprite = sprite;  
            }
        }

    }
}