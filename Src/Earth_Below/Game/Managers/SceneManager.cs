using GameManager.Base;
using System.Collections.Generic;

namespace GameManager.Managers;

public class SceneManager
{
    private readonly Stack<IScene> sceneStack;

    public SceneManager()
    {
        sceneStack = new();
    }

    public void AddScene(IScene scene)
    {
        scene.Load();
        sceneStack.Push(scene);
    }

    public void RemoveScene(int i)
    {
        while (i > 0)
        {
            sceneStack.Pop();
            i--;
        }
    }

    public IScene GetCurrentScene()
    {
        return sceneStack.Peek();
    }

    public bool IsStackEmpty()
    {
        return sceneStack.Count == 0;
    }
}