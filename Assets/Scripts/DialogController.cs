using System.Linq;
using System.Text;
using RolePlayCharacter;
using UnityEngine;
using UnityEngine.UI;
using WellFormedNames;

namespace Assets.Scripts
{
	public class DialogController : MonoBehaviour
	{
	    [SerializeField]
        private string m_charLabel;

		[SerializeField]
		private Text m_emotionFieldOne = null;

		[SerializeField]
		private Text m_moodFieldOne = null;

		[SerializeField]
		private Text m_dialogOne = null;

        private string currentLine = "";
		
		public void SetCharacterLabel(string text)
		{
            m_charLabel = text;
		}

		public void UpdateFields(RolePlayCharacterAsset rpc)
		{
			StringBuilder builder = new StringBuilder();
			bool notFirst = false;

			var query = rpc.GetAllActiveEmotions().GroupBy(e => e.Type).Select(g => g.OrderByDescending(e => e.Intensity).First()).OrderByDescending(e => e.Intensity);
			foreach (var emt in query)
			{
				if (notFirst)
					builder.AppendLine();
				builder.AppendFormat("{0}: {1:N2}", emt.Type, emt.Intensity);
				notFirst = true;
			}
			m_emotionFieldOne.text= builder.ToString();
			m_moodFieldOne.text = string.Format("Mood: {0:N2}", rpc.Mood);
		}

		public void AddDialogLine(string line, Name evt = null)
		{

            if(line != currentLine){

                currentLine = line;
            //small hack to draw the text background in the agent dialogue
            GameObject.Find("TextBackground1").GetComponent<Image>().enabled = true;
            //    GameObject.Find("TextBackground2").GetComponent<Image>().enabled = true;

            //  enterDialog(string.Format(@"<i>{0}: {1}</i>", m_charLabel, line), evt);
            enterDialog(string.Format(@"<i>{1}</i>", m_charLabel, line), evt);
            }
        }

		public void Clear()
		{
            //small hack to draw the text background in the agent dialogue
            GameObject.Find("TextBackground1").GetComponent<Image>().enabled = false;
          //  GameObject.Find("TextBackground2").GetComponent<Image>().enabled = false;

            m_dialogOne.text = string.Empty;
		}

		private void enterDialog(string line, Name evt)
		{
			if (evt != null)
				line += string.Format("\n<color=green>{0}</color>", evt);

			m_dialogOne.text = line;
		}
	}
}