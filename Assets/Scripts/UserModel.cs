using Firebase.Firestore;

[FirestoreData]
public class UserModel
{
    [FirestoreProperty]
    public string UserID { get; set; }

    [FirestoreProperty]
    public string UserName { get; set; }

    [FirestoreProperty]
    public string UserEmail { get; set; }

    [FirestoreProperty]
    public string UserImage { get; set; }

    [FirestoreProperty]
    public string UserTeacher { get; set; }
}