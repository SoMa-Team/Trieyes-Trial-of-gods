using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class TileCombiner : EditorWindow
{
    private string sourcePath = "Assets/Trieyes/Data/Environment/Sprites/Tiles/Subtiles";
    private string outputPath = "Assets/Trieyes/Data/Environment/Sprites/Tiles/Maintiles";
    private string outputFileName = "Combined_Tile_15x15.png";
    private int subtileSize = 1024; // 기본 타일 크기 (필요에 따라 조정)
    private int gridSize = 15; // 10x10 그리드

    [MenuItem("Tools/Tile Combiner")]
    public static void ShowWindow()
    {
        GetWindow<TileCombiner>("Tile Combiner");
    }

    private void OnGUI()
    {
        GUILayout.Label("타일 조합기", EditorStyles.boldLabel);
        
        sourcePath = EditorGUILayout.TextField("소스 경로:", sourcePath);
        outputPath = EditorGUILayout.TextField("출력 경로:", outputPath);
        outputFileName = EditorGUILayout.TextField("출력 파일명:", outputFileName);
        subtileSize = EditorGUILayout.IntField("타일 크기:", subtileSize);
        gridSize = EditorGUILayout.IntField("그리드 크기:", gridSize);

        GUILayout.Space(10);

        if (GUILayout.Button("타일 조합하기"))
        {
            CombineTiles();
        }

        GUILayout.Space(10);
        GUILayout.Label("사용법:", EditorStyles.boldLabel);
        GUILayout.Label("1. 소스 경로에 타일 PNG 파일들이 있어야 합니다.");
        GUILayout.Label("2. 타일 크기를 올바르게 설정하세요.");
        GUILayout.Label("3. '타일 조합하기' 버튼을 클릭하세요.");
    }

    private void CombineTiles()
    {
        // PNG 파일들 가져오기
        string[] pngFiles = Directory.GetFiles(sourcePath, "*.png");
        
        if (pngFiles.Length == 0)
        {
            EditorUtility.DisplayDialog("오류", "소스 경로에서 PNG 파일을 찾을 수 없습니다.", "확인");
            return;
        }

        // 텍스처 로드
        Texture2D[] sourceTextures = new Texture2D[pngFiles.Length];
        for (int i = 0; i < pngFiles.Length; i++)
        {
            sourceTextures[i] = LoadTextureFromFile(pngFiles[i]);
            if (sourceTextures[i] == null)
            {
                EditorUtility.DisplayDialog("오류", $"텍스처를 로드할 수 없습니다: {pngFiles[i]}", "확인");
                return;
            }
        }

        // 결과 텍스처 생성
        int resultWidth = subtileSize * gridSize;
        int resultHeight = subtileSize * gridSize;
        Texture2D resultTexture = new Texture2D(resultWidth, resultHeight, TextureFormat.RGBA32, false);

        // 각 그리드 위치에 타일 배치
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // 랜덤 타일 선택
                int randomTileIndex = Random.Range(0, sourceTextures.Length);
                Texture2D sourceTile = sourceTextures[randomTileIndex];

                // 랜덤 회전 (0, 90, 180, 270도)
                int rotation = Random.Range(0, 4) * 90;
                
                // 타일을 결과 텍스처에 복사
                CopyTileToResult(sourceTile, resultTexture, x, y, rotation);
            }
        }

        // 결과 텍스처를 파일로 저장
        string outputFilePath = Path.Combine(outputPath, outputFileName);
        SaveTextureToFile(resultTexture, outputFilePath);

        // 메모리 정리
        for (int i = 0; i < sourceTextures.Length; i++)
        {
            if (sourceTextures[i] != null)
                DestroyImmediate(sourceTextures[i]);
        }
        DestroyImmediate(resultTexture);

        // 에셋 데이터베이스 새로고침
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("완료", $"타일 조합이 완료되었습니다!\n저장 위치: {outputFilePath}", "확인");
    }

    private Texture2D LoadTextureFromFile(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        
        if (texture.LoadImage(fileData))
        {
            return texture;
        }
        
        DestroyImmediate(texture);
        return null;
    }

    private void CopyTileToResult(Texture2D sourceTile, Texture2D resultTexture, int gridX, int gridY, int rotation)
    {
        int startX = gridX * subtileSize;
        int startY = gridY * subtileSize;

        // 소스 타일을 타일 크기로 리사이즈
        Texture2D resizedTile = ResizeTexture(sourceTile, subtileSize, subtileSize);
        
        // 회전 적용
        Texture2D rotatedTile = RotateTexture(resizedTile, rotation);

        // 결과 텍스처에 복사
        for (int x = 0; x < subtileSize; x++)
        {
            for (int y = 0; y < subtileSize; y++)
            {
                Color pixelColor = rotatedTile.GetPixel(x, y);
                resultTexture.SetPixel(startX + x, startY + y, pixelColor);
            }
        }

        // 임시 텍스처 정리
        DestroyImmediate(resizedTile);
        DestroyImmediate(rotatedTile);
    }

    private Texture2D ResizeTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        Graphics.Blit(source, rt);
        
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;
        
        Texture2D result = new Texture2D(targetWidth, targetHeight);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();
        
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }

    private Texture2D RotateTexture(Texture2D source, int rotation)
    {
        int width = source.width;
        int height = source.height;
        
        Texture2D result = new Texture2D(width, height);
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color pixel;
                
                switch (rotation)
                {
                    case 90:
                        pixel = source.GetPixel(y, width - 1 - x);
                        break;
                    case 180:
                        pixel = source.GetPixel(width - 1 - x, height - 1 - y);
                        break;
                    case 270:
                        pixel = source.GetPixel(height - 1 - y, x);
                        break;
                    default: // 0도
                        pixel = source.GetPixel(x, y);
                        break;
                }
                
                result.SetPixel(x, y, pixel);
            }
        }
        
        result.Apply();
        return result;
    }

    private void SaveTextureToFile(Texture2D texture, string filePath)
    {
        byte[] pngData = texture.EncodeToPNG();
        File.WriteAllBytes(filePath, pngData);
    }
}
