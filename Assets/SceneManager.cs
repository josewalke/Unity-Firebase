using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using TMPro;
using Firebase.Firestore;
using System;

public class SceneManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;
    public TextMeshProUGUI email;
    public TextMeshProUGUI password;
    public TextMeshProUGUI listUser;
    public string allUsersText = "";
    List<bool> ListState = new List<bool>();
    List<string> ListEmail = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        // Inicializar la instancia de FirebaseAuth
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        
        InvokeRepeating("GetCollection", 2f, 2f);
        InvokeRepeating("UpdateListUserText", 2.5f, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        // Verificar si se presiona el botón ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Salir de la aplicación
            Exit();
        }

    }

    public void SignIn()
    {
        auth.SignInWithEmailAndPasswordAsync(email.text, password.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign-in failed: " + task.Exception);
                return;
            }

            // Obtener el resultado de la tarea
            AuthResult authResult = task.Result;

            // Extraer el FirebaseUser del AuthResult
            FirebaseUser user = authResult.User;

            Debug.Log("Signed in as: " + user.Email);
            UpdateUserData(true);
        });
    }
    public void SignUp()
    {
        auth.CreateUserWithEmailAndPasswordAsync(email.text, password.text).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Sign-up failed: " + task.Exception);
                return;
            }


            AuthResult authResult = task.Result;

            // Extraer el FirebaseUser del AuthResult
            FirebaseUser newUser = authResult.User;

            Debug.Log("New user created: " + newUser.Email);
            Dictionary<string, object> userData = new Dictionary<string, object>
            {
                { "email", email.text },
                { "state", true }
            };
            CreateUserData(newUser.UserId, userData);
        });
    }

    void GetCollection()
    {
        // Obtener la referencia a la colección
        CollectionReference collectionRef = db.Collection("Usuarios");

        // Realizar una consulta para obtener todos los documentos de la colección
        allUsersText = "";
        collectionRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to fetch documents: " + task.Exception);
                return;
            }
            // Iterar sobre los documentos obtenidos
            QuerySnapshot snapshot = task.Result;
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // Obtener los datos del documento como un diccionario
                    Dictionary<string, object> data = document.ToDictionary();
                    
                    foreach (KeyValuePair<string, object> d in data)
                    {
                        
                        if(d.Key == "email")
                        {
                            ListEmail.Add(d.Value.ToString());
                        }
                        else
                        {
                            ListState.Add(Convert.ToBoolean(d.Value));
                        }
                    }
                    
                    // Agregar el texto al campo listUser
                     DictionaryToString(data);
                    
                }
                else
                {
                    Debug.Log("Document does not exist: " + document.Id);
                }
            }
        });

    }

    public void UpdateListUserText()
    {
        if (listUser != null)
        {
            listUser.SetText(allUsersText);
            // Forzar la actualización de la interfaz de usuario
            Canvas.ForceUpdateCanvases();
        }
        else
        {
            Debug.LogError("listUserText is not assigned!");
        }
    }
    
    // Método auxiliar para convertir un diccionario a una cadena de texto
    string DictionaryToString(Dictionary<string, object> dictionary)
    {
        string result = "";
        foreach (KeyValuePair<string, object> pair in dictionary)
        {
            result +=  "" + pair.Value.ToString() + " ";
        }
        result += "";
        allUsersText += result;
        return result;
    }
    public void CreateUserData(string userId, Dictionary<string, object> data)
    {
        DocumentReference docRef = db.Collection("Usuarios").Document(userId);
        docRef.SetAsync(data).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.LogError("Failed to create user data: " + task.Exception);
                return;
            }

            Debug.Log("User data created successfully for user ID: " + userId);
        });
    }

    public void UpdateUserData(bool state)
    {
        
        // Obtener el ID del usuario actualmente autenticado
        string userId = auth.CurrentUser.UserId;
        Dictionary<string, object> newData = new Dictionary<string, object>
        {
            { "state", state }
            // Agrega más campos y valores según sea necesario
        };
        // Referencia al documento que se actualizará
        DocumentReference docRef = db.Collection("Usuarios").Document(userId);
        // Realiza una consulta en la colección utilizando el método Where()
        docRef.UpdateAsync(newData).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // Error al actualizar el documento
                Debug.LogError("Failed to update document: " + task.Exception);
                return;
            }

            // Documento actualizado exitosamente
            Debug.Log("Document updated successfully ");

        });
    }
    void Exit()
    {
        // Salir de la aplicación
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_WEBGL
            // No hacer nada en WebGL, ya que no se puede cerrar la aplicación desde JavaScript
#else
        // Salir de la aplicación
        Application.Quit();
#endif
    }
}
