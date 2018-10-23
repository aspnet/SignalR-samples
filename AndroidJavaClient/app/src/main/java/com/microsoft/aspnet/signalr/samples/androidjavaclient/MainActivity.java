package com.microsoft.aspnet.signalr.samples.androidjavaclient;


import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;

import com.microsoft.signalr.HubConnection;
import com.microsoft.signalr.HubConnectionBuilder;

import java.util.ArrayList;
import java.util.List;

public class MainActivity extends AppCompatActivity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        HubConnection hubConnection = HubConnectionBuilder.create("http://signalr-samples.azurewebsites.net/default").build();
        TextView textView = (TextView)findViewById(R.id.tvMain);
        ListView listView = (ListView)findViewById(R.id.lvMessages);
        Button sendButton = (Button)findViewById(R.id.bSend);
        EditText editText = (EditText)findViewById(R.id.etMessageText);

        List<String> messageList = new ArrayList<String>();
        ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(MainActivity.this,
                android.R.layout.simple_list_item_1, messageList);
        listView.setAdapter(arrayAdapter);

        hubConnection.on("Send", (message)-> {
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    arrayAdapter.add(message);
                    arrayAdapter.notifyDataSetChanged();
                }
            });
        }, String.class);

        sendButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String message = editText.getText().toString();
                editText.setText("");
                try {
                    hubConnection.send("Send", message);
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }
        });

        new HubConnectionTask().execute(hubConnection);
    }

    class HubConnectionTask extends AsyncTask<HubConnection, Void, Void>{

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
}