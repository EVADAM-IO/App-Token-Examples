import hashlib
import hmac
import json
import logging
import time
import uuid
import requests

EVADAM_SECRET_KEY = "YOUR_EVADAM_SECRET_KEY"  # Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq
EVADAM_APP_TOKEN = "YOUR_EVADAM_APP_TOKEN"  # Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad
EVADAM_TEST_BASE_URL = "https://api.evadam.io"
# Please don't forget to change token and secret key values to production ones when switching to production

def create_applicant(external_user_id):
    body = {'externalUserId': external_user_id}
    headers = {
        'Content-Type': 'application/json',
        'Content-Encoding': 'utf-8'
    }
    resp = sign_request(
        requests.Request('POST', EVADAM_TEST_BASE_URL + '/api/session/createSession',
                         data=json.dumps(body),
                         headers=headers))
    s = requests.Session()
    response = s.send(resp)
    applicant_id = (response.json()['id'])
    return applicant_id



def get_applicant_status(applicant_id):
    url = EVADAM_TEST_BASE_URL + '/api/applicants/' + applicant_id + '/'
    resp = sign_request(requests.Request('GET', url))
    s = requests.Session()
    response = s.send(resp)
    return response


def sign_request(request: requests.Request) -> requests.PreparedRequest:
    prepared_request = request.prepare()
    now = int(time.time())
    method = request.method.upper()
    path_url = prepared_request.path_url
    # could be None so we use an empty **byte** string here
    body = b'' if prepared_request.body is None else prepared_request.body
    if type(body) == str:
        body = body.encode('utf-8')
    data_to_sign = str(now).encode('utf-8') + method.encode('utf-8') + path_url.encode('utf-8') + body
    # hmac needs bytes
    signature = hmac.new(
        EVADAM_SECRET_KEY.encode('utf-8'),
        data_to_sign,
        digestmod=hashlib.sha256
    )
    prepared_request.headers['X-App-Token'] = EVADAM_APP_TOKEN
    prepared_request.headers['X-App-Access-Ts'] = str(now)
    prepared_request.headers['X-App-Access-Sig'] = signature.hexdigest()
    return prepared_request

def main():
    logging.basicConfig(level=logging.INFO)
    external_user_id = str(uuid.uuid4())
    applicant_id = create_applicant(external_user_id)
    logging.info(applicant_id)
    status = get_applicant_status(applicant_id)
    logging.info(status)



if __name__ == '__main__':
    exit(main())
