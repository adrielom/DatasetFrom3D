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
        
        public const string IMAGE_URL = "https://source.unsplash.com/1024x728/?interiors,home,inside";
        public Sprite sprite;
        public Image image;
        private Texture2D currentTexture;
        private UnityWebRequest currentRequest;


        public Background(Image image)
        {
            this.image = image;
        }

        public IEnumerator GetTexture() {

            currentRequest = UnityWebRequestTexture.GetTexture(IMAGE_URL);
            yield return currentRequest.SendWebRequest();
            if(currentRequest.isNetworkError || currentRequest.isHttpError) {
                
            }
            else {
                Texture newTexture = ((DownloadHandlerTexture)currentRequest.downloadHandler).texture;
                currentTexture = newTexture.ToTexture2D();
                sprite = Sprite.Create(currentTexture, new Rect(0.0f, 0.0f, currentTexture.width, currentTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
                image.sprite = sprite;
                Object.DestroyImmediate(newTexture);
            }
        }

    }
}