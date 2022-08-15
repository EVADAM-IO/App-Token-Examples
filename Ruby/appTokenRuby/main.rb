require 'json'
require 'rest-client'
require_relative 'model/applicant'
require_relative 'model/fixed_info'
require_relative 'model/metadata'
require_relative 'model/id_doc_types'
require_relative 'model/jsonable'
require 'securerandom'

APP_TOKEN = 'YOUR_EVADAM_APP_TOKEN'.freeze # Example: uY0CgwELmgUAEyl4hNWxLngb.0WSeQeiYny4WEqmAALEAiK2qTC96fBad
SECRET_KEY = 'YOUR_EVADAM_SECRET_KEY'.freeze # Example: Hej2ch71kG2kTd1iIUDZFNsO5C1lh5Gq
# Please don't forget to change token and secret key values to production ones when switching to production

def request_env_url(resource)
  "https://api.evadam.io/api/#{resource}"
end


def create_applicant(external_used_id, lang)
  resources = "session/createSessoion"
  body = Applicant.new(external_used_id, lang).serialize
  puts body
  header = signed_header(resources, body)

  response = RestClient.post(
    request_env_url(resources),
    body,
    header
  )
end


def get_applicant_status(applicant_id)
  resources = "applicants/#{applicant_id}/"
  RestClient.get request_env_url(resources), signed_header(resources, nil, 'GET')
end



def signed_header(resource, body = nil, method = 'POST', content_type = 'application/json')
  epoch_time = Time.now.to_i
  access_signature = signed_message(epoch_time, resource, body, method)
  {
    "X-App-Token": APP_TOKEN.encode('UTF-8').to_s,
    "X-App-Access-Sig": access_signature.encode('UTF-8').to_s,
    "X-App-Access-Ts": epoch_time.to_s.encode('UTF-8').to_s,
    "Accept": 'application/json',
    "Content-Type": content_type.to_s
  }
end


def signed_message(time, resource, body, method = 'POST')
  key = SECRET_KEY
  body_encoded = body.to_s if body
  data = "#{time.to_s}#{method}/api/#{resource.to_s}#{body_encoded.to_s}"
  digest = OpenSSL::Digest.new('sha256')
  OpenSSL::HMAC.hexdigest(digest, key, data)
end

uuid = SecureRandom.uuid
lang = 'en'

# 1. create an applicant
response = JSON.parse(create_applicant(uuid, lang))
puts("applicant_id #{response['id']}")
applicant_id = response['id']

# 2. getting applicant status
puts(get_applicant_status(applicant_id))

