import 'dart:convert';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../core/constants/api_constants.dart';
import '../core/constants/app_constants.dart';
import '../models/models.dart';
import 'api_client.dart';

class AuthService {
  final ApiClient _apiClient = ApiClient();
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  Future<AuthResponse> login(LoginRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.login,
      data: request.toJson(),
    );

    final authResponse = AuthResponse.fromJson(response.data);
    await _saveAuthData(authResponse);
    return authResponse;
  }

  Future<AuthResponse> register(RegisterRequest request) async {
    final response = await _apiClient.post(
      ApiConstants.register,
      data: request.toJson(),
    );

    final authResponse = AuthResponse.fromJson(response.data);
    await _saveAuthData(authResponse);
    return authResponse;
  }

  Future<User> getCurrentUser() async {
    final response = await _apiClient.get(ApiConstants.me);
    return User.fromJson(response.data);
  }

  Future<void> logout() async {
    await _apiClient.clearToken();
  }

  Future<bool> isLoggedIn() async {
    return await _apiClient.hasToken();
  }

  Future<User?> getSavedUser() async {
    final userJson = await _storage.read(key: AppConstants.userKey);
    if (userJson != null) {
      return User.fromJson(jsonDecode(userJson));
    }
    return null;
  }

  Future<void> _saveAuthData(AuthResponse authResponse) async {
    await _apiClient.setToken(authResponse.token);
    await _storage.write(
      key: AppConstants.userKey,
      value: jsonEncode(authResponse.user.toJson()),
    );
  }
}
