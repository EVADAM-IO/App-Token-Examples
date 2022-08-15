import com.fasterxml.jackson.databind.ObjectMapper;
import model.Applicant;
import model.DocType;
import model.HttpMethod;
import model.Metadata;
import okhttp3.MediaType;
import okhttp3.MultipartBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.RequestBody;
import okhttp3.Response;
import okhttp3.ResponseBody;
import okio.Buffer;
import org.apache.commons.codec.binary.Hex;

import javax.crypto.Mac;
import javax.crypto.spec.SecretKeySpec;
import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import java.security.InvalidKeyException;
import java.security.NoSuchAlgorithmException;
import java.time.Instant;
import java.util.UUID;

public class AppTokenJavaExample {
    private static final String EVADAM_SECRET_KEY = "YOUR_EVADAM_SECRET_KEY"; // Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq
    private static final String EVADAM_APP_TOKEN = "YOUR_EVADAM_APP_TOKEN"; // Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad
    private static final String EVADAM_TEST_BASE_URL = "https://api.evadam.io";
    //Please don't forget to change token and secret key values to production ones when switching to production

    private static final ObjectMapper objectMapper = new ObjectMapper();

    public static void main(String[] args) throws IOException, InvalidKeyException, NoSuchAlgorithmException {


        String externalUserId = UUID.randomUUID().toString();

        String applicantId = createApplicant(externalUserId);
        System.out.println("The applicant (" + externalUserId + ") was successfully created: " + applicantId);


        String applicantStatusStr = getApplicantStatus(applicantId);
        System.out.println("Applicant status (json string): " + applicantStatusStr);

    }

    public static String createApplicant(String externalUserId, String levelName) throws IOException, NoSuchAlgorithmException, InvalidKeyException {
        Applicant applicant = new Applicant(externalUserId);

        Response response = sendPost(
                "/api/session/createSession",
                RequestBody.create(
                        objectMapper.writeValueAsString(applicant),
                        MediaType.parse("application/json; charset=utf-8")));

        ResponseBody responseBody = response.body();

        return responseBody != null ? objectMapper.readValue(responseBody.string(), Applicant.class).getId() : null;
    }



    public static String getApplicantStatus(String applicantId) throws NoSuchAlgorithmException, InvalidKeyException, IOException {

        Response response = sendGet("/api/applicants/" + applicantId + "/");

        ResponseBody responseBody = response.body();
        return responseBody != null ? responseBody.string() : null;
    }

    private static Response sendPost(String url, RequestBody requestBody) throws IOException, InvalidKeyException, NoSuchAlgorithmException {
        long ts = Instant.now().getEpochSecond();

        Request request = new Request.Builder()
                .url(EVADAM_TEST_BASE_URL + url)
                .header("X-App-Token", EVADAM_APP_TOKEN)
                .header("X-App-Access-Sig", createSignature(ts, HttpMethod.POST, url, requestBodyToBytes(requestBody)))
                .header("X-App-Access-Ts", String.valueOf(ts))
                .post(requestBody)
                .build();

        Response response = new OkHttpClient().newCall(request).execute();

        if (response.code() != 200 && response.code() != 201) {
            // Then perhaps you should throw the exception. (depends on the logic of your code)
        }
        return response;
    }

    private static Response sendGet(String url) throws IOException, InvalidKeyException, NoSuchAlgorithmException {
        long ts = Instant.now().getEpochSecond();

        Request request = new Request.Builder()
                .url(EVADAM_TEST_BASE_URL + url)
                .header("X-App-Token", EVADAM_APP_TOKEN)
                .header("X-App-Access-Sig", createSignature(ts, HttpMethod.GET, url, null))
                .header("X-App-Access-Ts", String.valueOf(ts))
                .get()
                .build();

        Response response = new OkHttpClient().newCall(request).execute();

        if (response.code() != 200 && response.code() != 201) {
            // Then perhaps you should throw the exception. (depends on the logic of your code)
        }
        return response;
    }

    private static String createSignature(long ts, HttpMethod httpMethod, String path, byte[] body) throws NoSuchAlgorithmException, InvalidKeyException {
        Mac hmacSha256 = Mac.getInstance("HmacSHA256");
        hmacSha256.init(new SecretKeySpec(EVADAM_SECRET_KEY.getBytes(StandardCharsets.UTF_8), "HmacSHA256"));
        hmacSha256.update((ts + httpMethod.name() + path).getBytes(StandardCharsets.UTF_8));
        byte[] bytes = body == null ? hmacSha256.doFinal() : hmacSha256.doFinal(body);
        return Hex.encodeHexString(bytes);
    }

    public static byte[] requestBodyToBytes(RequestBody requestBody) throws IOException {
        Buffer buffer = new Buffer();
        requestBody.writeTo(buffer);
        return buffer.readByteArray();
    }

}
