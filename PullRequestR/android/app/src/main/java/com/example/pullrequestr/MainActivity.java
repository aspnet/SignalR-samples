package com.example.pullrequestr;
import android.content.Context;
import android.os.AsyncTask;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.*;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;
import com.squareup.picasso.Picasso;
import java.util.ArrayList;

public class MainActivity extends AppCompatActivity {

    public class PullRequest {
        public String Url;
        public Integer PullRequestId;
        public String Avatar;
        public String Login;
        public String Title;
    }

    ArrayList<PullRequest> pullRequests = new ArrayList<PullRequest>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        // This URL does NOT need to be suffixed with the "negotiate" endpoint, 
        // the /api endpoint is correct. 
        String url = "https://YOUR-FUNCTION-URI.azurewebsites.net/api";
        PullRequestAdapter adapter = new PullRequestAdapter(this, pullRequests);
        ListView lvItems = (ListView)findViewById(R.id.lvPullRequestList);

        HubConnection hubConnection = HubConnectionBuilder
                .create(url)
                .build();

        hubConnection.on("pullRequestOpened", (pullRequest)-> {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    pullRequests.add(pullRequest);
                    lvItems.setAdapter(adapter);
                }
            });
        }, PullRequest.class);

        new HubConnectionTask().execute(hubConnection);
    }

    class HubConnectionTask extends AsyncTask<HubConnection, Void, Void> {
        @Override
        protected void onPreExecute() {
            super.onPreExecute();
        }
        @Override
        protected Void doInBackground(HubConnection... hubConnections) {
            HubConnection hubConnection = hubConnections[0];
            hubConnection.start().blockingAwait();
            return null;
        }
    }

    public class PullRequestAdapter extends ArrayAdapter<PullRequest> {
        public PullRequestAdapter(Context context, ArrayList<PullRequest> pullRequests){
            super(context, 0, pullRequests);
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            PullRequest pr = getItem(position);
            if(convertView == null) {
                convertView = LayoutInflater.from(getContext())
                        .inflate(R.layout.pullrequestlayout, parent, false);
            }

            TextView tvTitle = (TextView)convertView.findViewById(R.id.tvTitle);
            tvTitle.setText(pr.Title);

            TextView tvPullRequestNumber = (TextView)convertView.findViewById(R.id.tvPullRequestNumber);
            tvPullRequestNumber.setText(pr.PullRequestId.toString());

            TextView tvLogin = (TextView)convertView.findViewById(R.id.tvLogin);
            tvLogin.setText(pr.Login);

            TextView tvUrl = (TextView)convertView.findViewById(R.id.tvUrl);
            tvUrl.setText(pr.Url);

            ImageView icon = (ImageView)convertView.findViewById(R.id.icon);
            Picasso.get().load(pr.Avatar).into(icon );

            return convertView;
        }
    }
}
