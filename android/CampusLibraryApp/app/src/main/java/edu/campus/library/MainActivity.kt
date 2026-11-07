package edu.campus.library

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.BackHandler
import androidx.activity.compose.setContent
import androidx.activity.enableEdgeToEdge
import androidx.compose.foundation.clickable
import androidx.compose.foundation.isSystemInDarkTheme
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.runtime.saveable.rememberSaveable
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.semantics.LiveRegionMode
import androidx.compose.ui.semantics.liveRegion
import androidx.compose.ui.semantics.semantics
import androidx.compose.ui.unit.dp
import androidx.lifecycle.createSavedStateHandle
import androidx.lifecycle.viewmodel.compose.viewModel
import androidx.lifecycle.viewmodel.initializer
import androidx.lifecycle.viewmodel.viewModelFactory
import androidx.lifecycle.compose.collectAsStateWithLifecycle
import java.time.Instant
import java.time.ZoneId
import java.time.format.DateTimeFormatter

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        enableEdgeToEdge()
        setContent {
            val colors = if (isSystemInDarkTheme()) darkColorScheme(
                primary = Color(0xFF9FD4BB), background = Color(0xFF101512), surface = Color(0xFF101512)
            ) else lightColorScheme(
                primary = Color(0xFF205B49), onPrimary = Color.White,
                secondary = Color(0xFF52645B), background = Color(0xFFF8FAF7),
                surface = Color(0xFFF8FAF7))
            MaterialTheme(colorScheme = colors) {
                val model: LibraryViewModel = viewModel(factory = viewModelFactory {
                    initializer { LibraryViewModel(LibraryRepository.create(), createSavedStateHandle()) }
                })
                LibraryApp(model)
            }
        }
    }
}

@OptIn(ExperimentalMaterial3Api::class)
@Composable
private fun LibraryApp(model: LibraryViewModel) {
    val state by model.state.collectAsStateWithLifecycle()
    var tab by rememberSaveable { mutableIntStateOf(0) }
    var bookId by rememberSaveable { mutableStateOf<Int?>(null) }
    var choosingMember by rememberSaveable { mutableStateOf(false) }
    val book = state.books.firstOrNull { it.id == bookId }
    BackHandler(bookId != null) { bookId = null }
    Scaffold(
        topBar = {
            TopAppBar(title = { Text(if (bookId != null) "Book details" else "CampusLibrary") },
                navigationIcon = { if (bookId != null) TextButton(onClick = { bookId = null }) { Text("Back") } },
                actions = { TextButton(onClick = model::refresh, enabled = !state.loading && !state.borrowing) { Text("Refresh") } })
        },
        bottomBar = {
            PrimaryTabRow(selectedTabIndex = tab) {
                Tab(selected = tab == 0, onClick = { tab = 0; bookId = null }, text = { Text("Catalog") })
                Tab(selected = tab == 1, onClick = { tab = 1; bookId = null }, text = { Text("My loans") })
            }
        }
    ) { padding ->
        LazyColumn(modifier = Modifier.fillMaxSize().padding(padding),
            contentPadding = PaddingValues(horizontal = 20.dp, vertical = 16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)) {
            item {
                Text("Development member", style = MaterialTheme.typography.titleSmall)
                Text("Choose a member for borrowing. This is a development selector; no sign-in is active.",
                    style = MaterialTheme.typography.bodyMedium)
                OutlinedButton(onClick = { choosingMember = true }, enabled = !state.loading && !state.borrowing) {
                    Text(state.members.firstOrNull { it.id == state.memberId }?.fullName ?: "Choose member")
                }
            }
            if (state.loading) item { LinearProgressIndicator(Modifier.fillMaxWidth()); Text("Loading library…") }
            state.error?.let { error -> item {
                Surface(color = MaterialTheme.colorScheme.errorContainer, shape = MaterialTheme.shapes.medium) {
                    Column(Modifier.padding(16.dp).semantics { liveRegion = LiveRegionMode.Polite }) {
                        Text(error, color = MaterialTheme.colorScheme.onErrorContainer)
                        TextButton(onClick = model::refresh, enabled = !state.loading && !state.borrowing) { Text("Try again") }
                    }
                }
            } }
            state.notice?.let { notice -> item {
                Text(notice, color = MaterialTheme.colorScheme.primary,
                    modifier = Modifier.semantics { liveRegion = LiveRegionMode.Polite })
            } }
            if (bookId != null) {
                if (book != null) {
                    item {
                        Text(book.title, style = MaterialTheme.typography.headlineMedium)
                        Spacer(Modifier.height(8.dp))
                        Text(book.author, style = MaterialTheme.typography.titleMedium)
                        Spacer(Modifier.height(24.dp))
                        Text("ISBN ${book.isbn}", style = MaterialTheme.typography.bodyMedium)
                        Availability(book)
                        Spacer(Modifier.height(16.dp))
                        Button(onClick = { model.borrow(book.id) }, modifier = Modifier.fillMaxWidth(),
                            enabled = book.isAvailable && state.memberId != null && !state.borrowing && !state.loading) {
                            Text(if (state.borrowing) "Borrowing…" else "Borrow book")
                        }
                        if (state.memberId == null) Text("Choose a development member above to borrow.")
                    }
                } else if (!state.loading) item { Text("This book is no longer in the catalog. Go back and refresh.") }
            } else if (tab == 0) {
                item { Text("Explore the catalog", style = MaterialTheme.typography.headlineMedium) }
                if (!state.loading && state.books.isEmpty() && state.error == null) item {
                    Text("No books yet. Ask your librarian to add books to the catalog.")
                }
                items(state.books, key = { it.id }) { entry ->
                    Column(Modifier.fillMaxWidth().clickable(onClickLabel = "View ${entry.title}") { bookId = entry.id }.padding(vertical = 12.dp)) {
                        Text(entry.title, style = MaterialTheme.typography.titleLarge)
                        Text(entry.author, style = MaterialTheme.typography.bodyLarge)
                        Availability(entry)
                    }
                    HorizontalDivider()
                }
            } else {
                item { Text("My loans", style = MaterialTheme.typography.headlineMedium) }
                if (!state.loading && state.loans.isEmpty() && state.error == null) item {
                    Text(if (state.memberId == null) "Choose a member to see their borrowing history." else "No loans yet. Browse the catalog to borrow your first book.")
                }
                items(state.loans.sortedByDescending { it.borrowedAt }, key = { it.id }) { loan ->
                    Column(Modifier.fillMaxWidth().padding(vertical = 8.dp)) {
                        Text(loan.bookTitle, style = MaterialTheme.typography.titleLarge)
                        Text(if (loan.returnedAt == null) "On loan" else "Returned", style = MaterialTheme.typography.labelLarge)
                        Text("Borrowed ${displayDate(loan.borrowedAt)}")
                        loan.returnedAt?.let { Text("Returned ${displayDate(it)}") }
                    }
                    HorizontalDivider()
                }
            }
        }
    }
    if (choosingMember) AlertDialog(onDismissRequest = { choosingMember = false },
        title = { Text("Choose development member") },
        text = {
            LazyColumn {
                if (state.members.isEmpty()) item { Text("No members found. Ask your librarian to create one.") }
                items(state.members, key = { it.id }) { member ->
                    TextButton(onClick = { model.selectMember(member.id); choosingMember = false }, modifier = Modifier.fillMaxWidth()) {
                        Column(Modifier.fillMaxWidth()) { Text(member.fullName); Text(member.email, style = MaterialTheme.typography.bodySmall) }
                    }
                }
            }
        }, confirmButton = { TextButton(onClick = { choosingMember = false }) { Text("Close") } })
}

@Composable
private fun Availability(book: Book) {
    Text(if (book.isAvailable) "Available to borrow" else "Currently unavailable",
        color = if (book.isAvailable) MaterialTheme.colorScheme.primary else MaterialTheme.colorScheme.onSurfaceVariant,
        style = MaterialTheme.typography.labelLarge, modifier = Modifier.padding(top = 8.dp))
}

private fun displayDate(value: String): String = runCatching {
    val utc = if (value.endsWith("Z") || value.contains('+')) value else value + "Z"
    DateTimeFormatter.ofPattern("d MMM yyyy, HH:mm").withZone(ZoneId.systemDefault()).format(Instant.parse(utc))
}.getOrDefault(value.take(10))
