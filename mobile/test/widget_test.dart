import 'package:flutter_test/flutter_test.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:brainstorming_app/app.dart';

void main() {
  testWidgets('App launches smoke test', (WidgetTester tester) async {
    // Build our app and trigger a frame.
    await tester.pumpWidget(
      const ProviderScope(
        child: BrainstormingApp(),
      ),
    );

    // Wait for any async operations
    await tester.pumpAndSettle();

    // Basic smoke test - app should launch without crashing
    expect(find.byType(BrainstormingApp), findsOneWidget);
  });
}
